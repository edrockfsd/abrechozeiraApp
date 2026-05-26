using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ABrechozeiraApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ABrechozeiraApp.Services;

/// <summary>
/// Serviço para emissão de NFC-e usando ACBrLib
/// </summary>
public class NfceService
{
    private readonly AbrechozeiraContext _context;
    private readonly ILogger<NfceService> _logger;

    public NfceService(AbrechozeiraContext context, ILogger<NfceService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Obtém as configurações fiscais da empresa
    /// </summary>
    public async Task<EmpresaFiscal?> GetConfigAsync()
    {
        return await _context.EmpresaFiscal.FirstOrDefaultAsync(e => e.Ativo);
    }

    /// <summary>
    /// Verifica se as configurações fiscais estão completas
    /// </summary>
    public async Task<(bool valido, string? erro)> ValidarConfiguracaoAsync()
    {
        var config = await GetConfigAsync();
        if (config == null)
            return (false, "Configurações fiscais não encontradas.");

        if (string.IsNullOrEmpty(config.CNPJ))
            return (false, "CNPJ não configurado.");

        if (string.IsNullOrEmpty(config.InscricaoEstadual))
            return (false, "Inscrição Estadual não configurada.");

        if (string.IsNullOrEmpty(config.CSC))
            return (false, "CSC não configurado. Gere no portal da SEFAZ.");

        if (string.IsNullOrEmpty(config.CertificadoPath) || !File.Exists(config.CertificadoPath))
            return (false, "Certificado digital não configurado ou arquivo não encontrado.");

        if (config.CertificadoValidade.HasValue && config.CertificadoValidade < DateTime.UtcNow)
            return (false, "Certificado digital expirado.");

        return (true, null);
    }

    /// <summary>
    /// Emite NFC-e a partir de uma venda do PDV
    /// </summary>
    public async Task<Nfce> EmitirNfceVendaPdvAsync(int vendaPdvId)
    {
        // Validar configuração
        var (valido, erro) = await ValidarConfiguracaoAsync();
        if (!valido)
            throw new InvalidOperationException(erro);

        var config = await GetConfigAsync();
        if (config == null)
            throw new InvalidOperationException("Configurações fiscais não encontradas.");

        // Buscar venda com itens e pagamentos
        var venda = await _context.VendaPdv
            .Include(v => v.Cliente)
            .FirstOrDefaultAsync(v => v.Id == vendaPdvId);

        if (venda == null)
            throw new ArgumentException($"Venda PDV {vendaPdvId} não encontrada.");

        if (venda.Status != "Finalizada")
            throw new InvalidOperationException("Apenas vendas finalizadas podem gerar NFC-e.");

        // Verificar se já existe NFC-e para esta venda
        var nfceExistente = await _context.Nfce.FirstOrDefaultAsync(n => n.VendaPdvId == vendaPdvId);
        if (nfceExistente != null)
            throw new InvalidOperationException($"Já existe NFC-e emitida para esta venda: {nfceExistente.ChaveAcesso}");

        // Buscar itens e pagamentos
        var itensVenda = await _context.VendaPdvItem
            .Include(i => i.Produto)
            .Where(i => i.VendaPdvId == vendaPdvId)
            .ToListAsync();

        var pagamentosVenda = await _context.VendaPdvPagamento
            .Where(p => p.VendaPdvId == vendaPdvId)
            .ToListAsync();

        // Criar NFC-e
        var nfce = new Nfce
        {
            Numero = config.ProximoNumero,
            Serie = config.Serie,
            Ambiente = config.Ambiente,
            VendaPdvId = vendaPdvId,
            ClienteId = venda.ClienteId,
            ValorProdutos = venda.ValorBruto,
            ValorDesconto = venda.Desconto,
            ValorTotal = venda.ValorLiquido,
            Status = "Pendente",
            DataEmissao = DateTime.UtcNow,
            UsuarioId = venda.UsuarioId
        };

        // Mapear itens
        var itensNfce = new List<NfceItem>();
        int numItem = 1;
        foreach (var item in itensVenda)
        {
            itensNfce.Add(new NfceItem
            {
                NumeroItem = numItem++,
                ProdutoId = item.ProdutoId,
                CodigoProduto = item.ProdutoId?.ToString() ?? item.Id.ToString(),
                Descricao = item.DescricaoItem,
                NCM = "00000000", // TODO: Configurar NCM no produto
                CFOP = "5102", // Venda de mercadoria adquirida
                Unidade = "UN",
                Quantidade = item.Quantidade,
                ValorUnitario = item.PrecoUnitario,
                ValorDesconto = item.DescontoValor,
                ValorTotal = item.Total,
                CSOSN = config.CRT == 4 ? "102" : "102", // MEI ou Simples Nacional
                OrigemMercadoria = 0
            });
        }

        // Mapear pagamentos
        var pagamentosNfce = new List<NfcePagamento>();
        foreach (var pg in pagamentosVenda)
        {
            pagamentosNfce.Add(new NfcePagamento
            {
                TipoPagamento = MapearFormaPagamento(pg.FormaPagamentoId),
                Valor = pg.Valor,
                TipoIntegracao = 2 // Não integrado
            });
        }

        nfce.Itens = itensNfce;
        nfce.Pagamentos = pagamentosNfce;

        // TODO: Integrar com ACBrLib para gerar XML, assinar e enviar à SEFAZ
        // Por enquanto, simula a criação
        _logger.LogInformation("Preparando NFC-e {Numero} para venda PDV {VendaId}", nfce.Numero, vendaPdvId);

        // Simulação - em ambiente de homologação
        if (config.Ambiente == 2)
        {
            nfce.ChaveAcesso = GerarChaveAcessoSimulada(config, nfce);
            nfce.Status = "Autorizada";
            nfce.Protocolo = "000000000000000";
            nfce.DataAutorizacao = DateTime.UtcNow;
            nfce.MensagemRetorno = "Autorizado o uso da NF-e (SIMULAÇÃO HOMOLOGAÇÃO)";
            nfce.CodigoRetorno = 100;
        }

        // Salvar
        _context.Nfce.Add(nfce);

        // Atualizar próximo número
        config.ProximoNumero++;
        config.DataAlteracao = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("NFC-e {ChaveAcesso} emitida com sucesso para venda PDV {VendaId}", nfce.ChaveAcesso, vendaPdvId);

        return nfce;
    }

    /// <summary>
    /// Emite NFC-e a partir de um pedido
    /// </summary>
    public async Task<Nfce> EmitirNfcePedidoAsync(int pedidoId)
    {
        // Validar configuração
        var (valido, erro) = await ValidarConfiguracaoAsync();
        if (!valido)
            throw new InvalidOperationException(erro);

        var config = await GetConfigAsync();
        if (config == null)
            throw new InvalidOperationException("Configurações fiscais não encontradas.");

        // Buscar pedido
        var pedido = await _context.Pedido
            .Include(p => p.Cliente)
            .FirstOrDefaultAsync(p => p.Id == pedidoId);

        if (pedido == null)
            throw new ArgumentException($"Pedido {pedidoId} não encontrado.");

        // Verificar se já existe NFC-e para este pedido
        var nfceExistente = await _context.Nfce.FirstOrDefaultAsync(n => n.PedidoId == pedidoId);
        if (nfceExistente != null)
            throw new InvalidOperationException($"Já existe NFC-e emitida para este pedido: {nfceExistente.ChaveAcesso}");

        // Buscar itens do pedido
        var itensPedido = await _context.PedidoProduto
            .Include(i => i.Produto)
            .Where(i => i.PedidoId == pedidoId)
            .ToListAsync();

        // Calcular totais
        var valorProdutos = itensPedido.Sum(i => i.ValorFinalProduto ?? 0);
        var valorTotal = pedido.ValorTotal ?? valorProdutos;

        // Criar NFC-e
        var nfce = new Nfce
        {
            Numero = config.ProximoNumero,
            Serie = config.Serie,
            Ambiente = config.Ambiente,
            PedidoId = pedidoId,
            ClienteId = pedido.ClienteID,
            ValorProdutos = valorProdutos,
            ValorTotal = valorTotal,
            Status = "Pendente",
            DataEmissao = DateTime.UtcNow
        };

        // Mapear itens
        var itensNfce = new List<NfceItem>();
        int numItem = 1;
        foreach (var item in itensPedido)
        {
            itensNfce.Add(new NfceItem
            {
                NumeroItem = numItem++,
                ProdutoId = item.ProdutoId,
                CodigoProduto = item.ProdutoId.ToString(),
                Descricao = item.Produto?.Descricao ?? "Produto",
                NCM = "00000000",
                CFOP = "5102",
                Unidade = "UN",
                Quantidade = item.Quantidade,
                ValorUnitario = item.ValorFinalProduto ?? 0,
                ValorTotal = (item.ValorFinalProduto ?? 0) * item.Quantidade,
                CSOSN = "102",
                OrigemMercadoria = 0
            });
        }

        // Pagamento padrão (dinheiro) - TODO: integrar com forma de pagamento do pedido
        var pagamentosNfce = new List<NfcePagamento>
        {
            new NfcePagamento
            {
                TipoPagamento = "01", // Dinheiro
                Valor = valorTotal,
                TipoIntegracao = 2
            }
        };

        nfce.Itens = itensNfce;
        nfce.Pagamentos = pagamentosNfce;

        _logger.LogInformation("Preparando NFC-e {Numero} para pedido {PedidoId}", nfce.Numero, pedidoId);

        // Simulação - em ambiente de homologação
        if (config.Ambiente == 2)
        {
            nfce.ChaveAcesso = GerarChaveAcessoSimulada(config, nfce);
            nfce.Status = "Autorizada";
            nfce.Protocolo = "000000000000000";
            nfce.DataAutorizacao = DateTime.UtcNow;
            nfce.MensagemRetorno = "Autorizado o uso da NF-e (SIMULAÇÃO HOMOLOGAÇÃO)";
            nfce.CodigoRetorno = 100;
        }

        // Salvar
        _context.Nfce.Add(nfce);
        config.ProximoNumero++;
        config.DataAlteracao = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("NFC-e {ChaveAcesso} emitida com sucesso para pedido {PedidoId}", nfce.ChaveAcesso, pedidoId);

        return nfce;
    }

    /// <summary>
    /// Cancela uma NFC-e
    /// </summary>
    public async Task<Nfce> CancelarNfceAsync(int nfceId, string justificativa)
    {
        if (string.IsNullOrWhiteSpace(justificativa) || justificativa.Length < 15)
            throw new ArgumentException("Justificativa deve ter no mínimo 15 caracteres.");

        var nfce = await _context.Nfce.FindAsync(nfceId);
        if (nfce == null)
            throw new ArgumentException($"NFC-e {nfceId} não encontrada.");

        if (nfce.Status != "Autorizada")
            throw new InvalidOperationException("Apenas NFC-e autorizadas podem ser canceladas.");

        // Verificar prazo (24 horas)
        if (nfce.DataAutorizacao.HasValue && DateTime.UtcNow > nfce.DataAutorizacao.Value.AddHours(24))
            throw new InvalidOperationException("NFC-e só pode ser cancelada em até 24 horas após a autorização.");

        // TODO: Integrar com ACBrLib para enviar evento de cancelamento à SEFAZ

        var config = await GetConfigAsync();
        if (config?.Ambiente == 2)
        {
            // Simulação em homologação
            nfce.Status = "Cancelada";
            nfce.DataCancelamento = DateTime.UtcNow;
            nfce.JustificativaCancelamento = justificativa;
            nfce.ProtocoloCancelamento = "000000000000001";
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("NFC-e {ChaveAcesso} cancelada. Justificativa: {Justificativa}", nfce.ChaveAcesso, justificativa);

        return nfce;
    }

    /// <summary>
    /// Lista NFC-e com filtros
    /// </summary>
    public async Task<List<Nfce>> ListarNfceAsync(DateTime? inicio, DateTime? fim, string? status, int limite = 50)
    {
        var query = _context.Nfce.AsQueryable();

        if (inicio.HasValue)
            query = query.Where(n => n.DataEmissao >= inicio.Value);

        if (fim.HasValue)
            query = query.Where(n => n.DataEmissao <= fim.Value);

        if (!string.IsNullOrEmpty(status))
            query = query.Where(n => n.Status == status);

        return await query
            .OrderByDescending(n => n.DataEmissao)
            .Take(limite)
            .ToListAsync();
    }

    /// <summary>
    /// Obtém NFC-e por ID com itens e pagamentos
    /// </summary>
    public async Task<Nfce?> GetNfceAsync(int id)
    {
        return await _context.Nfce
            .Include(n => n.Itens)
            .Include(n => n.Pagamentos)
            .Include(n => n.Cliente)
            .FirstOrDefaultAsync(n => n.Id == id);
    }

    /// <summary>
    /// Gera chave de acesso simulada para ambiente de homologação
    /// </summary>
    private string GerarChaveAcessoSimulada(EmpresaFiscal config, Nfce nfce)
    {
        var cUF = ObterCodigoUF(config.UF);
        var dataEmissao = nfce.DataEmissao.ToString("yyMM");
        var cnpj = config.CNPJ.PadLeft(14, '0');
        var mod = "65"; // NFC-e
        var serie = nfce.Serie.ToString().PadLeft(3, '0');
        var numero = nfce.Numero.ToString().PadLeft(9, '0');
        var tpEmis = config.TipoEmissao.ToString();
        var cNF = new Random().Next(10000000, 99999999).ToString();

        var chave = $"{cUF}{dataEmissao}{cnpj}{mod}{serie}{numero}{tpEmis}{cNF}";

        // Calcular dígito verificador (simplificado)
        var dv = CalcularDV(chave);

        return chave + dv;
    }

    private string ObterCodigoUF(string uf)
    {
        var codigos = new Dictionary<string, string>
        {
            {"AC", "12"}, {"AL", "27"}, {"AP", "16"}, {"AM", "13"}, {"BA", "29"},
            {"CE", "23"}, {"DF", "53"}, {"ES", "32"}, {"GO", "52"}, {"MA", "21"},
            {"MT", "51"}, {"MS", "50"}, {"MG", "31"}, {"PA", "15"}, {"PB", "25"},
            {"PR", "41"}, {"PE", "26"}, {"PI", "22"}, {"RJ", "33"}, {"RN", "24"},
            {"RS", "43"}, {"RO", "11"}, {"RR", "14"}, {"SC", "42"}, {"SP", "35"},
            {"SE", "28"}, {"TO", "17"}
        };

        return codigos.TryGetValue(uf.ToUpper(), out var codigo) ? codigo : "43";
    }

    private string CalcularDV(string chave)
    {
        // Algoritmo Módulo 11
        var pesos = new[] { 2, 3, 4, 5, 6, 7, 8, 9 };
        var soma = 0;
        var idx = 0;

        for (int i = chave.Length - 1; i >= 0; i--)
        {
            soma += int.Parse(chave[i].ToString()) * pesos[idx % 8];
            idx++;
        }

        var resto = soma % 11;
        var dv = 11 - resto;

        if (dv >= 10) dv = 0;

        return dv.ToString();
    }

    private string MapearFormaPagamento(int? formaPagamentoId)
    {
        // Mapeamento básico - TODO: configurar mapeamento completo
        return formaPagamentoId switch
        {
            1 => "01", // Dinheiro
            2 => "03", // Cartão de Crédito
            3 => "04", // Cartão de Débito
            4 => "17", // PIX
            _ => "99"  // Outros
        };
    }
}
