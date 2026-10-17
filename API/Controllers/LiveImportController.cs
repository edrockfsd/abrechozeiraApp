using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ABrechozeiraApp.Models;
using ABrechozeiraApp.Services;
using Microsoft.Extensions.Logging;

namespace ABrechozeiraApp.Controllers;

[Route("api/[controller]")]
[ApiController]
[Microsoft.AspNetCore.Authorization.Authorize]
public class LiveImportController : ControllerBase
{
    private readonly AbrechozeiraContext _context;
    private readonly GoogleSheetReaderService _sheetReader;
    private readonly ProdutoIAService _produtoIA;
    private readonly CacheSistemaService _cacheSistema;
    private readonly VendaService _vendaService;
    private readonly ILogger<LiveImportController> _logger;

    public LiveImportController(
        AbrechozeiraContext context,
        GoogleSheetReaderService sheetReader,
        ProdutoIAService produtoIA,
        CacheSistemaService cacheSistema,
        VendaService vendaService,
        ILogger<LiveImportController> logger)
    {
        _context = context;
        _sheetReader = sheetReader;
        _produtoIA = produtoIA;
        _cacheSistema = cacheSistema;
        _vendaService = vendaService;
        _logger = logger;
    }

    /// <summary>
    /// Preview dos dados da planilha via Google Sheets URL com sugestões de vinculação de clientes
    /// </summary>
    [HttpGet("preview-url")]
    public async Task<IActionResult> PreviewUrl([FromQuery] string url, [FromQuery] string sheet = "vendas")
    {
        try
        {
            var linhas = await _sheetReader.LerPorUrlAsync(url, sheet);
            var (mapeamentos, clientes) = await GerarPreviewMapeamentosAsync(linhas);
            return Ok(new
            {
                totalLinhas = linhas.Count,
                compradores = mapeamentos.Count,
                linhas = linhas.Select(l => new
                {
                    l.CodigoLive,
                    l.Descricao,
                    l.Valor,
                    l.Comprador,
                    l.Fila,
                    l.LinhaOriginal
                }),
                mapeamentos,
                clientes
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { erro = ex.Message });
        }
    }

    /// <summary>
    /// Preview dos dados de arquivo .xlsx com sugestões de vinculação de clientes
    /// </summary>
    [HttpPost("preview-xlsx")]
    public async Task<IActionResult> PreviewXlsx([FromForm] IFormFile arquivo)
    {
        if (arquivo == null || arquivo.Length == 0)
            return BadRequest(new { erro = "Nenhum arquivo enviado." });

        try
        {
            using var stream = arquivo.OpenReadStream();
            var linhas = _sheetReader.LerPorXlsx(stream);
            var (mapeamentos, clientes) = await GerarPreviewMapeamentosAsync(linhas);
            return Ok(new
            {
                totalLinhas = linhas.Count,
                compradores = mapeamentos.Count,
                linhas = linhas.Select(l => new
                {
                    l.CodigoLive,
                    l.Descricao,
                    l.Valor,
                    l.Comprador,
                    l.Fila,
                    l.LinhaOriginal
                }),
                mapeamentos,
                clientes
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { erro = ex.Message });
        }
    }

    /// <summary>
    /// Gera a lista de compradores únicos da planilha e cruza com os clientes cadastrados no sistema
    /// </summary>
    private async Task<(List<MapeamentoClienteDto> Mapeamentos, List<ClienteComboDto> Clientes)> GerarPreviewMapeamentosAsync(List<LinhaArremate> linhas)
    {
        var clientes = await _context.Pessoa
            .Where(p => p.StatusId == 1)
            .OrderBy(p => p.Nome)
            .Select(p => new ClienteComboDto
            {
                Id = p.Id,
                Nome = p.Nome ?? "",
                NickName = p.NickName ?? "",
                Cpf = p.CPF
            })
            .ToListAsync();

        var grupos = linhas
            .Where(l => !string.IsNullOrWhiteSpace(l.Comprador))
            .GroupBy(l => l.Comprador.Trim(), StringComparer.OrdinalIgnoreCase)
            .ToList();

        var mapeamentos = new List<MapeamentoClienteDto>();

        foreach (var g in grupos)
        {
            var comprador = g.Key;
            var totalPecas = g.Count();
            var valorTotal = g.Sum(l => l.Valor);

            var cClean = comprador.TrimStart('@').ToLower();
            var cAlnum = new string(cClean.Where(char.IsLetterOrDigit).ToArray());

            // 1. Busca exata (NickName com ou sem @)
            var match = clientes.FirstOrDefault(c =>
            {
                var nickClean = (c.NickName ?? "").Trim().TrimStart('@').ToLower();
                return nickClean == cClean;
            });
            string tipoMatch = "EXATO";

            // 2. Busca alfanumérica (ignora . e _)
            if (match == null && !string.IsNullOrEmpty(cAlnum))
            {
                match = clientes.FirstOrDefault(c =>
                {
                    var nickAlnum = new string((c.NickName ?? "").Where(char.IsLetterOrDigit).ToArray()).ToLower();
                    return nickAlnum == cAlnum;
                });
                if (match != null) tipoMatch = "EXATO";
            }

            // 3. Busca por similaridade ou substring
            if (match == null)
            {
                match = clientes.FirstOrDefault(c =>
                {
                    var nickClean = (c.NickName ?? "").Trim().TrimStart('@').ToLower();
                    var nomeClean = (c.Nome ?? "").Trim().ToLower();
                    return (nickClean.Length >= 4 && cClean.Length >= 4 && (nickClean.Contains(cClean) || cClean.Contains(nickClean)))
                        || (nomeClean.Length >= 4 && cClean.Length >= 4 && nomeClean.Contains(cClean));
                });
                if (match != null) tipoMatch = "SIMILAR";
            }

            // 4. Busca por distância Levenshtein (erros de digitação: letras duplicadas, trocadas)
            if (match == null && cClean.Length >= 5)
            {
                match = clientes
                    .Select(c => new
                    {
                        Cliente = c,
                        NickClean = (c.NickName ?? "").Trim().TrimStart('@').ToLower()
                    })
                    .Where(x => x.NickClean.Length >= 5 && Math.Abs(x.NickClean.Length - cClean.Length) <= 2)
                    .Select(x => new { x.Cliente, Dist = LevenshteinDistance(cClean, x.NickClean) })
                    .Where(x => x.Dist <= 2)
                    .OrderBy(x => x.Dist)
                    .Select(x => x.Cliente)
                    .FirstOrDefault();

                if (match != null) tipoMatch = "SIMILAR";
            }

            if (match != null)
            {
                mapeamentos.Add(new MapeamentoClienteDto
                {
                    CompradorPlanilha = comprador,
                    PessoaId = match.Id,
                    TotalPecas = totalPecas,
                    ValorTotal = valorTotal,
                    SugeridoPessoaId = match.Id,
                    SugeridoNome = match.Nome,
                    SugeridoNick = match.NickName,
                    SugeridoCpf = match.Cpf,
                    TipoMatch = tipoMatch
                });
            }
            else
            {
                mapeamentos.Add(new MapeamentoClienteDto
                {
                    CompradorPlanilha = comprador,
                    PessoaId = null,
                    TotalPecas = totalPecas,
                    ValorTotal = valorTotal,
                    SugeridoPessoaId = null,
                    SugeridoNome = null,
                    SugeridoNick = null,
                    SugeridoCpf = null,
                    TipoMatch = "NOVO"
                });
            }
        }

        return (mapeamentos.OrderBy(m => m.CompradorPlanilha).ToList(), clientes);
    }

    private static int LevenshteinDistance(string s, string t)
    {
        if (string.IsNullOrEmpty(s)) return t?.Length ?? 0;
        if (string.IsNullOrEmpty(t)) return s.Length;

        int n = s.Length;
        int m = t.Length;
        int[,] d = new int[n + 1, m + 1];

        for (int i = 0; i <= n; d[i, 0] = i++) ;
        for (int j = 0; j <= m; d[0, j] = j++) ;

        for (int i = 1; i <= n; i++)
        {
            for (int j = 1; j <= m; j++)
            {
                int cost = (t[j - 1] == s[i - 1]) ? 0 : 1;
                d[i, j] = Math.Min(
                    Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1),
                    d[i - 1, j - 1] + cost);
            }
        }
        return d[n, m];
    }

    /// <summary>
    /// Importa dados da planilha via URL da Google Sheet
    /// </summary>
    [HttpPost("importar-url")]
    public async Task<IActionResult> ImportarPorUrl([FromBody] ImportarUrlRequest request)
    {
        try
        {
            var linhas = await _sheetReader.LerPorUrlAsync(request.GoogleSheetUrl, request.SheetName ?? "vendas");
            var resultado = await ProcessarImportacao(linhas, request.LiveId, request.Mapeamentos);
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro na importação por URL");
            return StatusCode(500, new { erro = $"Erro na importação: {ex.Message}" });
        }
    }

    /// <summary>
    /// Importa dados de arquivo .xlsx com mapeamento opcional de clientes
    /// </summary>
    [HttpPost("importar-xlsx")]
    public async Task<IActionResult> ImportarPorXlsx([FromForm] IFormFile arquivo, [FromForm] int liveId, [FromForm] string? mapeamentosJson = null)
    {
        if (arquivo == null || arquivo.Length == 0)
            return BadRequest(new { erro = "Nenhum arquivo enviado." });

        try
        {
            using var stream = arquivo.OpenReadStream();
            var linhas = _sheetReader.LerPorXlsx(stream);

            List<MapeamentoClienteDto>? mapeamentos = null;
            if (!string.IsNullOrWhiteSpace(mapeamentosJson))
            {
                try
                {
                    mapeamentos = System.Text.Json.JsonSerializer.Deserialize<List<MapeamentoClienteDto>>(
                        mapeamentosJson,
                        new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Falha ao desserializar mapeamentosJson: {Msg}", ex.Message);
                }
            }

            var resultado = await ProcessarImportacao(linhas, liveId, mapeamentos);
            return Ok(resultado);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro na importação por XLSX");
            return StatusCode(500, new { erro = $"Erro na importação: {ex.Message}" });
        }
    }

    /// <summary>
    /// Consulta o status da Live para saber se permite nova importação ou se está bloqueada por NFC-e autorizada
    /// </summary>
    [HttpGet("status-live/{liveId}")]
    [Microsoft.AspNetCore.Authorization.AllowAnonymous]
    public async Task<IActionResult> GetStatusLive(int liveId)
    {
        var live = await _context.Live.FindAsync(liveId);
        if (live == null) return NotFound(new { erro = $"Live {liveId} não encontrada." });

        var vendas = await _context.Venda.Where(v => v.LiveId == liveId).ToListAsync();
        var vendaIds = vendas.Select(v => v.Id).ToList();

        var nfcesAutorizadas = await _context.Nfce
            .Where(n => n.VendaId.HasValue && vendaIds.Contains(n.VendaId.Value) && n.Status == "Autorizada")
            .CountAsync();

        var arrematesProvisorios = await _context.Arremate
            .Where(a => a.LiveId == liveId && a.ProdutoId == null)
            .CountAsync();

        var arrematesOficiais = await _context.Arremate
            .Where(a => a.LiveId == liveId && a.ProdutoId != null)
            .CountAsync();

        var bloqueado = nfcesAutorizadas > 0;
        string? motivo = bloqueado 
            ? $"Esta Live possui {nfcesAutorizadas} nota(s) fiscal(is) (NFC-e) já autorizada(s) perante a SEFAZ. Por segurança contábil e fiscal, o reenvio da planilha de arremates está bloqueado."
            : null;

        return Ok(new
        {
            liveId,
            tituloLive = live.Titulo,
            totalVendas = vendas.Count,
            totalNfceAutorizadas = nfcesAutorizadas,
            totalArrematesProvisorios = arrematesProvisorios,
            totalArrematesOficiais = arrematesOficiais,
            bloqueadoParaImportacao = bloqueado,
            motivoBloqueio = motivo
        });
    }

    /// <summary>
    /// Limpa dados de teste gerados para uma Live específica (NFC-e, Vendas, Pedidos, Arremates, Estoque, Produtos)
    /// </summary>
    [HttpDelete("limpar-live/{liveId}")]
    [Microsoft.AspNetCore.Authorization.AllowAnonymous]
    public async Task<IActionResult> LimparDadosLive(int liveId)
    {
        try
        {
            var live = await _context.Live.FindAsync(liveId);
            if (live == null)
                return NotFound(new { erro = $"Live {liveId} não encontrada." });

            var vendas = await _context.Venda.Where(v => v.LiveId == liveId).ToListAsync();
            var vendaIds = vendas.Select(v => v.Id).ToList();

            // Checagem de segurança fiscal: se houver NFC-e autorizada, impedir exclusão
            var nfcesAutorizadas = await _context.Nfce
                .Where(n => n.VendaId.HasValue && vendaIds.Contains(n.VendaId.Value) && n.Status == "Autorizada")
                .CountAsync();

            if (nfcesAutorizadas > 0)
            {
                return BadRequest(new { 
                    erro = $"Operação cancelada: a Live {liveId} possui {nfcesAutorizadas} nota(s) fiscal(is) já autorizada(s) perante a SEFAZ. Não é permitido excluir os dados." 
                });
            }

            var resultado = await ExecutarLimpezaInterna(liveId, live);

            return Ok(new
            {
                mensagem = $"Dados da Live {liveId} ({live.Titulo}) resetados com sucesso.",
                vendasRemovidas = resultado.vendasRemovidas,
                nfcesRemovidas = resultado.nfcesRemovidas,
                pedidosRemovidos = resultado.pedidosRemovidos,
                arrematesRemovidos = resultado.arrematesRemovidos,
                produtosRemovidos = resultado.produtosRemovidos
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao limpar dados da Live {LiveId}", liveId);
            return StatusCode(500, new { erro = $"Erro ao limpar dados: {ex.Message}" });
        }
    }

    private async Task<(int vendasRemovidas, int nfcesRemovidas, int pedidosRemovidos, int arrematesRemovidos, int produtosRemovidos)> ExecutarLimpezaInterna(int liveId, Live live)
    {
        // 1. Obter Vendas e Pedidos da live
        var vendas = await _context.Venda.Where(v => v.LiveId == liveId).ToListAsync();
        var vendaIds = vendas.Select(v => v.Id).ToList();
        var pedidoIds = vendas.Select(v => v.PedidoId).Distinct().ToList();

        var outrosPedidos = await _context.Pedido
            .Where(p => p.Observacoes != null && (p.Observacoes.Contains($"Live - {live.Titulo}") || p.Observacoes.Contains($"Live {liveId}")))
            .Select(p => p.Id)
            .ToListAsync();
        pedidoIds = pedidoIds.Union(outrosPedidos).Distinct().ToList();

        // 2. Nfce geradas para essas vendas OU pedidos (apenas as que não forem autorizadas, por segurança)
        var nfces = await _context.Nfce
            .Where(n => ((n.VendaId.HasValue && vendaIds.Contains(n.VendaId.Value)) ||
                        (n.PedidoId.HasValue && pedidoIds.Contains(n.PedidoId.Value))) &&
                        n.Status != "Autorizada")
            .ToListAsync();
        var nfceIds = nfces.Select(n => n.Id).ToList();

        var nfcePagamentos = await _context.NfcePagamento.Where(p => nfceIds.Contains(p.NfceId)).ToListAsync();
        _context.NfcePagamento.RemoveRange(nfcePagamentos);

        var nfceItens = await _context.NfceItem.Where(i => nfceIds.Contains(i.NfceId)).ToListAsync();
        _context.NfceItem.RemoveRange(nfceItens);

        _context.Nfce.RemoveRange(nfces);
        await _context.SaveChangesAsync();

        // 3. Deletar Vendas
        _context.Venda.RemoveRange(vendas);
        await _context.SaveChangesAsync();

        // 4. Deletar Pedidos e PedidoProdutos
        var pedidoProdutos = await _context.PedidoProduto.Where(pp => pedidoIds.Contains(pp.PedidoId)).ToListAsync();
        _context.PedidoProduto.RemoveRange(pedidoProdutos);

        var pedidos = await _context.Pedido.Where(p => pedidoIds.Contains(p.Id)).ToListAsync();
        _context.Pedido.RemoveRange(pedidos);
        await _context.SaveChangesAsync();

        // 5. Deletar Arremates
        var arremates = await _context.Arremate.Where(a => a.LiveId == liveId).ToListAsync();
        var produtoIds = arremates.Where(a => a.ProdutoId.HasValue).Select(a => a.ProdutoId!.Value).Distinct().ToList();
        _context.Arremate.RemoveRange(arremates);
        await _context.SaveChangesAsync();

        // 6. Deletar Estoque e Produtos criados da Live
        var estoques = await _context.Estoque.Where(e => produtoIds.Contains(e.ProdutoId)).ToListAsync();
        _context.Estoque.RemoveRange(estoques);

        var produtos = await _context.Produto.Where(p => produtoIds.Contains(p.Id)).ToListAsync();
        _context.Produto.RemoveRange(produtos);
        await _context.SaveChangesAsync();

        // 7. Deletar Pessoas criadas automaticamente para esta live (sem CPF, sem telefone e sem outros pedidos)
        var pessoasDosPedidos = pedidos.Select(p => p.ClienteID).Distinct().ToList();
        var pessoasAutoCriadas = await _context.Pessoa
            .Where(p => pessoasDosPedidos.Contains(p.Id) &&
                        (p.CPF == null || p.CPF == "") &&
                        (p.Telefone == null || p.Telefone == "") &&
                        !_context.Pedido.Any(outro => outro.ClienteID == p.Id && !pedidoIds.Contains(outro.Id)))
            .ToListAsync();

        if (pessoasAutoCriadas.Any())
        {
            _context.Pessoa.RemoveRange(pessoasAutoCriadas);
            await _context.SaveChangesAsync();
        }

        return (vendas.Count, nfces.Count, pedidos.Count, arremates.Count, produtos.Count);
    }

    /// <summary>
    /// Processa a importação completa: Produto → Arremate → Pedido → Venda
    /// </summary>
    private async Task<object> ProcessarImportacao(List<LinhaArremate> linhas, int liveId, List<MapeamentoClienteDto>? mapeamentosCustomizados = null)
    {
        // Validar Live
        var live = await _context.Live.FindAsync(liveId);
        if (live == null)
            throw new ArgumentException($"Live {liveId} não encontrada.");

        var avisos = new List<string>();

        // REGRA DE NEGÓCIO 2: Se a live já tiver vendas com NFC-e autorizada, BLOQUEAR reimportação
        var vendasExistentes = await _context.Venda.Where(v => v.LiveId == liveId).ToListAsync();
        var vendaIdsExistentes = vendasExistentes.Select(v => v.Id).ToList();

        var nfcesAutorizadas = await _context.Nfce
            .Where(n => n.VendaId.HasValue && vendaIdsExistentes.Contains(n.VendaId.Value) && n.Status == "Autorizada")
            .CountAsync();

        if (nfcesAutorizadas > 0)
        {
            throw new InvalidOperationException(
                $"Importação bloqueada: A Live '{live.Titulo}' possui {nfcesAutorizadas} nota(s) fiscal(is) (NFC-e) já autorizada(s) perante a SEFAZ. Por segurança fiscal, não é permitido reenviar a planilha de arremates.");
        }

        // Se já existirem vendas anteriores (sem NFC-e autorizada), limpar os dados anteriores para não duplicar
        if (vendasExistentes.Any())
        {
            await ExecutarLimpezaInterna(liveId, live);
            avisos.Add("Dados e vendas anteriores da Live foram substituídos com sucesso.");
        }
        else
        {
            // REGRA DE NEGÓCIO 1: Se existirem arremates provisórios da sincronização online (ProdutoId == null), remover automaticamente
            var arrematesProvisorios = await _context.Arremate
                .Where(a => a.LiveId == liveId && a.ProdutoId == null)
                .ToListAsync();

            if (arrematesProvisorios.Any())
            {
                _context.Arremate.RemoveRange(arrematesProvisorios);
                await _context.SaveChangesAsync();
                avisos.Add($"Substituídos {arrematesProvisorios.Count} arremates provisórios da transmissão online pela planilha oficial.");
            }
        }

        // Carregar cache de domínio para IA
        await _cacheSistema.CarregarAsync();

        // Buscar/Criar origem "Live Instagram"
        var origem = await _context.Origem
            .FirstOrDefaultAsync(o => o.Descricao.Contains("Live Instagram") || o.Descricao.Contains("live instagram"));
        if (origem == null)
        {
            origem = new Origem { Descricao = "Live Instagram" };
            _context.Origem.Add(origem);
            await _context.SaveChangesAsync();
        }

        // Buscar FormaPagamento PIX e CondicaoPagamento À Vista
        var formaPix = await _context.FormaPagamento
            .FirstOrDefaultAsync(f => f.Descricao.Contains("PIX") || f.Descricao.Contains("Pix") || f.Descricao.Contains("pix"));
        var condicaoAVista = await _context.CondicaoPagamento
            .FirstOrDefaultAsync(c => c.Descricao.Contains("Vista") || c.Descricao.Contains("vista"));

        var produtosCadastrados = 0;
        var arrematesImportados = 0;
        var erros = new List<object>();

        // Identificar compradores desconsiderados (desistência de compra)
        var compradoresIgnorados = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (mapeamentosCustomizados != null && mapeamentosCustomizados.Any())
        {
            // 1. Explicitamente marcados como Ignorado
            foreach (var m in mapeamentosCustomizados.Where(m => m.Ignorado))
            {
                var comp = m.CompradorPlanilha?.Trim();
                if (!string.IsNullOrEmpty(comp))
                {
                    compradoresIgnorados.Add(comp);
                    compradoresIgnorados.Add(comp.TrimStart('@'));
                }
            }

            // 2. Compradores que foram completamente excluídos da lista pelo operador
            var compradoresPresentes = mapeamentosCustomizados
                .Where(m => !m.Ignorado)
                .Select(m => m.CompradorPlanilha?.Trim().TrimStart('@'))
                .Where(n => !string.IsNullOrEmpty(n))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            foreach (var l in linhas)
            {
                var cClean = (l.Comprador ?? "").Trim().TrimStart('@');
                if (!string.IsNullOrEmpty(cClean) && !compradoresPresentes.Contains(cClean))
                {
                    compradoresIgnorados.Add(cClean);
                    compradoresIgnorados.Add((l.Comprador ?? "").Trim());
                }
            }
        }

        // Mapeamento: comprador -> lista de (produtoId, valor, descricao)
        var arrematePorComprador = new Dictionary<string, List<(int produtoId, decimal valor, string descricao)>>(
            StringComparer.OrdinalIgnoreCase);

        // ===== ETAPA 1: Para cada linha, criar Produto + Estoque + Arremate =====
        foreach (var linha in linhas)
        {
            var compItem = (linha.Comprador ?? "").Trim();
            var compItemClean = compItem.TrimStart('@');

            // Pular compradores desconsiderados por desistência
            if (compradoresIgnorados.Contains(compItem) || compradoresIgnorados.Contains(compItemClean))
            {
                continue;
            }

            try
            {
                // Chamar IA para desmembrar descrição
                var dadosIA = await _produtoIA.ProcessarDescricaoAsync(linha.Descricao, linha.Valor);

                // Criar Produto
                var produto = new Produto
                {
                    Descricao = dadosIA.Descricao,
                    Tamanho = dadosIA.Tamanho,
                    GrupoID = dadosIA.GrupoId,
                    MarcaId = dadosIA.MarcaId,
                    GeneroID = dadosIA.GeneroId,
                    PerfilID = dadosIA.PerfilId,
                    Condicao = string.IsNullOrEmpty(dadosIA.Condicao) ? 'N' : dadosIA.Condicao[0],
                    PrecoVenda = dadosIA.PrecoVenda,
                    PrecoCusto = 0,
                    Origem = "Live Instagram",
                    StatusId = 1,
                    DataCompra = DateTime.UtcNow,
                    DataAlteracao = DateTime.UtcNow
                };
                _context.Produto.Add(produto);
                await _context.SaveChangesAsync();

                // Criar Estoque
                var estoque = new Estoque
                {
                    ProdutoId = produto.Id,
                    Quantidade = 1,
                    Localizacao = "Loja Principal",
                    DataAlteracao = DateTime.UtcNow
                };
                _context.Estoque.Add(estoque);

                // Criar Arremate (histórico)
                var arremate = new Arremate
                {
                    LiveId = liveId,
                    ProdutoId = produto.Id,
                    CodigoLive = linha.CodigoLive,
                    Arrematante = linha.Comprador,
                    ValorArremate = linha.Valor,
                    Fila = string.IsNullOrWhiteSpace(linha.Fila) ? null : linha.Fila,
                    DescricaoManual = null,
                    ImportadoPlanilha = true,
                    DataArremate = DateTime.UtcNow,
                    DataAlteracao = DateTime.UtcNow
                };
                _context.Arremate.Add(arremate);
                await _context.SaveChangesAsync();

                produtosCadastrados++;
                arrematesImportados++;

                // Agrupar por comprador
                var compradorKey = linha.Comprador.ToLower().Trim();
                if (!arrematePorComprador.ContainsKey(compradorKey))
                    arrematePorComprador[compradorKey] = new List<(int, decimal, string)>();

                arrematePorComprador[compradorKey].Add((produto.Id, linha.Valor, dadosIA.Descricao));

                if (linha.CodigoLive == null)
                    avisos.Add($"Linha {linha.LinhaOriginal}: Código Live ausente, importado mesmo assim");
            }
            catch (Exception ex)
            {
                erros.Add(new { linha = linha.LinhaOriginal, descricao = linha.Descricao, erro = ex.Message });
                _logger.LogWarning("Erro na linha {Linha}: {Erro}", linha.LinhaOriginal, ex.Message);
            }
        }

        if (compradoresIgnorados.Any())
        {
            var nomesIgnorados = compradoresIgnorados
                .Select(c => "@" + c.TrimStart('@'))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
            avisos.Add($"Desconsideradas as peças de {nomesIgnorados.Count} comprador(es) por desistência: {string.Join(", ", nomesIgnorados)}");
        }

        // ===== ETAPA 2: Para cada comprador, criar Pedido + PedidoProduto + Venda =====
        var pedidosGerados = 0;
        var vendasGeradas = 0;
        var detalhesPedidos = new List<object>();

        // Obter último PedidoCodigo
        var ultimoCodigo = _context.Pedido.Any() ? _context.Pedido.Max(p => p.PedidoCodigo) : 0;

        // Buscar categoria "Cliente" dinamicamente (Id 2)
        var categoriaCliente = await _context.PessoaCategoria.FirstOrDefaultAsync(c => c.Descricao == "Cliente")
                            ?? await _context.PessoaCategoria.FirstOrDefaultAsync(c => c.Id == 2);
        var categoriaClienteId = categoriaCliente?.Id ?? 2;

        // Buscar tipo "Física" dinamicamente (Id 1)
        var tipoFisica = await _context.PessoaTipo.FirstOrDefaultAsync(t => t.Descricao.Contains("Física") || t.Descricao.Contains("Fisica"))
                         ?? await _context.PessoaTipo.FirstOrDefaultAsync(t => t.Id == 1);
        var tipoFisicaId = tipoFisica?.Id ?? 1;

        // Buscar gênero padrão Feminino (Id 1)
        var generoFeminino = await _context.PessoaGenero.FirstOrDefaultAsync(g => g.Descricao == "Feminino" || g.Sigla == "F")
                             ?? await _context.PessoaGenero.FirstOrDefaultAsync(g => g.Id == 1);
        var generoFemininoId = generoFeminino?.Id ?? 1;

        var mapDict = new Dictionary<string, int?>(StringComparer.OrdinalIgnoreCase);
        if (mapeamentosCustomizados != null)
        {
            foreach (var m in mapeamentosCustomizados)
            {
                if (!string.IsNullOrWhiteSpace(m.CompradorPlanilha))
                    mapDict[m.CompradorPlanilha.Trim()] = m.PessoaId;
            }
        }

        foreach (var (comprador, itens) in arrematePorComprador)
        {
            try
            {
                var pessoaCriada = false;
                Pessoa? pessoa = null;

                // 1. Se o operador vinculou manualmente a uma Pessoa existente
                if (mapDict.TryGetValue(comprador, out var idEscolhido) && idEscolhido.HasValue && idEscolhido.Value > 0)
                {
                    pessoa = await _context.Pessoa.FirstOrDefaultAsync(p => p.Id == idEscolhido.Value);
                }

                // 2. Se não veio mapeado explicitamente, buscar por matching inteligente (com/sem @ e pontuação)
                if (pessoa == null)
                {
                    var cClean = comprador.Trim().TrimStart('@').ToLower();
                    var cAlnum = new string(cClean.Where(char.IsLetterOrDigit).ToArray());

                    // Busca exata (NickName com ou sem @)
                    pessoa = await _context.Pessoa
                        .FirstOrDefaultAsync(p => p.NickName != null &&
                            (p.NickName.ToLower().Trim() == cClean ||
                             p.NickName.ToLower().Trim() == "@" + cClean));

                    // Busca alfanumérica (ignora . e _)
                    if (pessoa == null && !string.IsNullOrEmpty(cAlnum))
                    {
                        var candidatos = await _context.Pessoa
                            .Where(p => p.NickName != null && p.StatusId == 1)
                            .ToListAsync();

                        pessoa = candidatos.FirstOrDefault(p =>
                        {
                            var nAlnum = new string((p.NickName ?? "").Where(char.IsLetterOrDigit).ToArray()).ToLower();
                            return nAlnum == cAlnum;
                        });
                    }
                }

                // 3. Se ainda assim não existe, criar Pessoa mínima com categoria "Cliente"
                if (pessoa == null)
                {
                    pessoa = new Pessoa
                    {
                        NickName = comprador.StartsWith("@") ? comprador : "@" + comprador,
                        Nome = comprador,
                        PessoaGeneroId = generoFemininoId,
                        PessoaCategoriaId = categoriaClienteId,
                        PessoaTipoId = tipoFisicaId,
                        StatusId = 1,
                        DataInclusao = DateTime.Now
                    };
                    _context.Pessoa.Add(pessoa);
                    await _context.SaveChangesAsync();
                    pessoaCriada = true;
                    avisos.Add($"Cliente '{comprador}' criado automaticamente (sem dados completos)");
                }

                ultimoCodigo++;

                // Criar Pedido
                var pedido = new Pedido
                {
                    PedidoCodigo = ultimoCodigo,
                    ClienteID = pessoa.Id,
                    DataLancamento = DateTime.UtcNow,
                    PedidoStatusID = 1, // Pendente (será atualizado ao gerar venda)
                    FormaPagamentoID = formaPix?.Id,
                    CondicaoPagamentoID = condicaoAVista?.Id,
                    ValorTotal = itens.Sum(i => i.valor),
                    Observacoes = $"Importado de Live - {live.Titulo}",
                    DataAlteracao = DateTime.UtcNow
                };
                _context.Pedido.Add(pedido);
                await _context.SaveChangesAsync();

                // Criar PedidoProduto para cada peça
                foreach (var (produtoId, valor, descricao) in itens)
                {
                    var pedidoProduto = new PedidoProduto
                    {
                        PedidoId = pedido.Id,
                        ProdutoId = produtoId,
                        Quantidade = 1,
                        ValorFinalProduto = valor,
                        DescontoValor = 0,
                        DataAlteracao = DateTime.UtcNow
                    };
                    _context.PedidoProduto.Add(pedidoProduto);
                }
                await _context.SaveChangesAsync();
                pedidosGerados++;

                // Gerar Venda a partir do Pedido
                var venda = await _vendaService.GerarVendaDePedidoAsync(pedido.Id, liveId, origem.Id);
                vendasGeradas++;

                detalhesPedidos.Add(new
                {
                    cliente = comprador,
                    pessoaCriada,
                    pessoaId = pessoa.Id,
                    pedidoId = pedido.Id,
                    pedidoCodigo = pedido.PedidoCodigo,
                    vendaId = venda.Id,
                    itens = itens.Count,
                    valorTotal = itens.Sum(i => i.valor),
                    descricaoItens = itens.Select(i => i.descricao).ToList()
                });
            }
            catch (Exception ex)
            {
                erros.Add(new { comprador, erro = ex.Message });
                _logger.LogWarning("Erro ao gerar pedido/venda para '{Comprador}': {Erro}", comprador, ex.Message);
            }
        }

        _logger.LogInformation(
            "Importação concluída: {Produtos} produtos, {Arremates} arremates, {Pedidos} pedidos, {Vendas} vendas",
            produtosCadastrados, arrematesImportados, pedidosGerados, vendasGeradas);

        return new
        {
            mensagem = "Importação concluída",
            produtosCadastrados,
            arrematesImportados,
            pedidosGerados,
            vendasGeradas,
            detalhesPedidos,
            erros,
            avisos
        };
    }
}

public class ImportarUrlRequest
{
    public int LiveId { get; set; }
    public string GoogleSheetUrl { get; set; } = string.Empty;
    public string? SheetName { get; set; } = "vendas";
    public List<MapeamentoClienteDto>? Mapeamentos { get; set; }
}

public class MapeamentoClienteDto
{
    public string CompradorPlanilha { get; set; } = string.Empty;
    public int? PessoaId { get; set; }
    public int TotalPecas { get; set; }
    public decimal ValorTotal { get; set; }
    public int? SugeridoPessoaId { get; set; }
    public string? SugeridoNome { get; set; }
    public string? SugeridoNick { get; set; }
    public string? SugeridoCpf { get; set; }
    public string? TipoMatch { get; set; } // "EXATO", "SIMILAR", "NOVO", "MANUAL"
    public bool Ignorado { get; set; } = false;
}

public class ClienteComboDto
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string NickName { get; set; } = string.Empty;
    public string? Cpf { get; set; }
}
