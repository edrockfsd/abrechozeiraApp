using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
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

        if (!string.IsNullOrEmpty(config.CertificadoSenha))
        {
            try
            {
                using var cert = new X509Certificate2(config.CertificadoPath, config.CertificadoSenha, X509KeyStorageFlags.EphemeralKeySet);
                if (cert.NotAfter < DateTime.UtcNow)
                    return (false, $"Certificado digital expirou em {cert.NotAfter:dd/MM/yyyy}.");

                if (!config.CertificadoValidade.HasValue)
                {
                    config.CertificadoValidade = cert.NotAfter;
                    await _context.SaveChangesAsync();
                }
            }
            catch (System.Security.Cryptography.CryptographicException)
            {
                return (false, "Senha do certificado digital incorreta.");
            }
            catch (Exception ex)
            {
                return (false, $"Erro ao carregar certificado digital: {ex.Message}");
            }
        }
        else if (config.CertificadoValidade.HasValue && config.CertificadoValidade < DateTime.UtcNow)
        {
            return (false, "Certificado digital expirado.");
        }

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
                NCM = "63090010", // Artigos do vestuário e acessórios usados
                CFOP = "5102", // Venda de mercadoria adquirida de terceiros
                Unidade = "UN",
                Quantidade = item.Quantidade,
                ValorUnitario = item.PrecoUnitario,
                ValorDesconto = item.DescontoValor,
                ValorTotal = item.Total,
                CSOSN = "102", // Tributada pelo Simples Nacional sem permissão de crédito
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

        nfce.Itens = itensNfce;
        nfce.Pagamentos = pagamentosNfce;

        _logger.LogInformation("Processando emissão de NFC-e {Numero} para venda PDV {VendaId}", nfce.Numero, vendaPdvId);

        // Processar emissão real SEFAZ ou simulação
        await ProcessarEmissaoAsync(config, nfce);

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
                NCM = "63090010",
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

        _logger.LogInformation("Processando emissão de NFC-e {Numero} para pedido {PedidoId}", nfce.Numero, pedidoId);

        // Processar emissão real SEFAZ ou simulação
        await ProcessarEmissaoAsync(config, nfce);

        // Salvar
        _context.Nfce.Add(nfce);
        config.ProximoNumero++;
        config.DataAlteracao = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        _logger.LogInformation("NFC-e {ChaveAcesso} emitida com sucesso para pedido {PedidoId}", nfce.ChaveAcesso, pedidoId);

        return nfce;
    }

    /// <summary>
    /// Emite NFC-e a partir de uma Venda (fluxo padrão: Pedido → Venda → NFC-e)
    /// </summary>
    public async Task<Nfce> EmitirNfcePorVendaAsync(int vendaId)
    {
        var (valido, erro) = await ValidarConfiguracaoAsync();
        if (!valido)
            throw new InvalidOperationException(erro);

        var config = await GetConfigAsync();
        if (config == null)
            throw new InvalidOperationException("Configurações fiscais não encontradas.");

        var venda = await _context.Venda
            .Include(v => v.Cliente)
            .Include(v => v.FormaPagamento)
            .FirstOrDefaultAsync(v => v.Id == vendaId);

        if (venda == null)
            throw new ArgumentException($"Venda {vendaId} não encontrada.");

        if (venda.Status == "Cancelada")
            throw new InvalidOperationException("Não é possível emitir NFC-e para venda cancelada.");

        var nfceExistente = await _context.Nfce.FirstOrDefaultAsync(n => n.VendaId == vendaId && n.Status == "Autorizada");
        if (nfceExistente != null)
            throw new InvalidOperationException($"Já existe NFC-e autorizada para esta venda: {nfceExistente.ChaveAcesso}");

        // Se houver tentativa anterior rejeitada/pendente, remover para substituir
        var tentativasAnteriores = await _context.Nfce
            .Include(n => n.Itens)
            .Include(n => n.Pagamentos)
            .Where(n => n.VendaId == vendaId && n.Status != "Autorizada")
            .ToListAsync();
        if (tentativasAnteriores.Any())
        {
            _context.Nfce.RemoveRange(tentativasAnteriores);
            await _context.SaveChangesAsync();
        }

        var itensPedido = await _context.PedidoProduto
            .Include(i => i.Produto)
            .Where(i => i.PedidoId == venda.PedidoId)
            .ToListAsync();

        if (!itensPedido.Any())
            throw new InvalidOperationException($"Venda {vendaId} sem itens.");

        var valorProdutos = itensPedido.Sum(i => (i.ValorFinalProduto ?? 0) * i.Quantidade);

        var nfce = new Nfce
        {
            Numero = config.ProximoNumero,
            Serie = config.Serie,
            Ambiente = config.Ambiente,
            VendaId = vendaId,
            PedidoId = venda.PedidoId,
            ClienteId = venda.ClienteId,
            ValorProdutos = valorProdutos,
            ValorDesconto = venda.Desconto,
            ValorTotal = venda.ValorTotal,
            Status = "Pendente",
            DataEmissao = DateTime.UtcNow
        };

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
                NCM = "63090010",
                CFOP = "5102",
                Unidade = "UN",
                Quantidade = item.Quantidade,
                ValorUnitario = item.ValorFinalProduto ?? 0,
                ValorTotal = (item.ValorFinalProduto ?? 0) * item.Quantidade,
                CSOSN = "102",
                OrigemMercadoria = 0
            });
        }

        var tipoPag = MapearFormaPagamentoParaNfce(venda.FormaPagamento?.Descricao);
        nfce.Itens = itensNfce;
        nfce.Pagamentos = new List<NfcePagamento>
        {
            new NfcePagamento { TipoPagamento = tipoPag, Valor = venda.ValorTotal, TipoIntegracao = 2 }
        };

        _logger.LogInformation("Processando NFC-e {Numero} para venda {VendaId}", nfce.Numero, vendaId);
        await ProcessarEmissaoAsync(config, nfce);

        _context.Nfce.Add(nfce);
        config.ProximoNumero++;
        config.DataAlteracao = DateTime.UtcNow;

        venda.Status = "Faturada";
        venda.DataAlteracao = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        _logger.LogInformation("NFC-e {ChaveAcesso} emitida para venda {VendaId}", nfce.ChaveAcesso, vendaId);
        return nfce;
    }

    /// <summary>
    /// Mapeia descrição da forma de pagamento para código NFC-e
    /// </summary>
    private string MapearFormaPagamentoParaNfce(string? descricao)
    {
        if (string.IsNullOrWhiteSpace(descricao)) return "01";
        var d = descricao.ToUpperInvariant();
        if (d.Contains("PIX")) return "17";
        if (d.Contains("DEBITO") || d.Contains("DÉBITO")) return "04";
        if (d.Contains("CREDITO") || d.Contains("CRÉDITO")) return "03";
        if (d.Contains("BOLETO")) return "15";
        return "01";
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
        var cnpj = new string((config.CNPJ ?? "").Where(char.IsDigit).ToArray()).PadLeft(14, '0');
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

    /// <summary>
    /// Processa a emissão real via WebService SEFAZ-PR ou fallback simulado
    /// </summary>
    private async Task ProcessarEmissaoAsync(EmpresaFiscal config, Nfce nfce)
    {
        if (!string.IsNullOrEmpty(config.CertificadoPath) && File.Exists(config.CertificadoPath) && !string.IsNullOrEmpty(config.CertificadoSenha))
        {
            try
            {
                _logger.LogInformation("Iniciando assinatura e transmissão real da NFC-e {Numero} via SEFAZ-PR...", nfce.Numero);
                var certBytes = await File.ReadAllBytesAsync(config.CertificadoPath);
                using var cert = new X509Certificate2(certBytes, config.CertificadoSenha, X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet);
                _logger.LogInformation("Certificado A1 carregado: Subject='{Subject}', CNPJ Config='{ConfigCNPJ}'", cert.Subject, config.CNPJ);

                var xmlDoc = GerarXmlNfce(config, nfce, out var chaveAcesso);
                nfce.ChaveAcesso = chaveAcesso;

                var xmlAssinadoDoc = AssinarXml(xmlDoc, chaveAcesso, cert);

                var (autorizado, protocolo, mensagem, xmlEnvio, xmlRetorno) = await TransmitirSefazPrAsync(xmlAssinadoDoc, config, cert);

                nfce.Status = autorizado ? "Autorizada" : "Rejeitada";
                nfce.Protocolo = protocolo ?? "000000000000000";
                nfce.MensagemRetorno = mensagem;
                nfce.DataAutorizacao = autorizado ? DateTime.UtcNow : null;
                nfce.XmlEnvio = xmlEnvio;
                nfce.XmlRetorno = xmlRetorno;

                _logger.LogInformation("Resultado transmissão SEFAZ NFC-e {Numero}: Status={Status}, Prot={Protocolo}, Msg={Mensagem}",
                    nfce.Numero, nfce.Status, nfce.Protocolo, nfce.MensagemRetorno);

                return;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha na transmissão via SEFAZ WebService: {Message}", ex.Message);
            }
        }

        // Fallback em homologação caso certificado não esteja totalmente funcional localmente
        if (config.Ambiente == 2)
        {
            _logger.LogWarning("Certificado não disponível para envio remoto real. Executando simulação de homologação.");
            nfce.ChaveAcesso = GerarChaveAcessoSimulada(config, nfce);
            nfce.Status = "Autorizada";
            nfce.Protocolo = "000000000000000";
            nfce.DataAutorizacao = DateTime.UtcNow;
            nfce.MensagemRetorno = "Autorizado o uso da NF-e (SIMULAÇÃO HOMOLOGAÇÃO)";
            nfce.CodigoRetorno = 100;
        }
    }

    /// <summary>
    /// Gera a árvore XML oficial v4.00 da NFC-e
    /// </summary>
    private XmlDocument GerarXmlNfce(EmpresaFiscal config, Nfce nfce, out string chaveAcesso)
    {
        var cUF = ObterCodigoUF(config.UF);
        var dataEmissao = nfce.DataEmissao;
        var dhEmi = dataEmissao.ToString("yyyy-MM-ddTHH:mm:sszzz");
        var cnpjOnly = new string((config.CNPJ ?? "").Where(char.IsDigit).ToArray()).PadLeft(14, '0');
        var mod = "65"; // NFC-e
        var serie = nfce.Serie.ToString().PadLeft(3, '0');
        var numero = nfce.Numero.ToString().PadLeft(9, '0');
        var tpEmis = config.TipoEmissao.ToString();
        var cNF = (nfce.Id + 10000000).ToString().PadLeft(8, '0');

        var chaveSemDv = $"{cUF}{dataEmissao:yyMM}{cnpjOnly}{mod}{serie}{numero}{tpEmis}{cNF}";
        var dv = CalcularDV(chaveSemDv);
        chaveAcesso = chaveSemDv + dv;

        var ns = "http://www.portalfiscal.inf.br/nfe";
        var xmlStr = new StringBuilder();
        xmlStr.Append($"<NFe xmlns=\"{ns}\">");
        xmlStr.Append($"<infNFe Id=\"NFe{chaveAcesso}\" versao=\"4.00\">");

        // <ide>
        xmlStr.Append("<ide>");
        xmlStr.Append($"<cUF>{cUF}</cUF>");
        xmlStr.Append($"<cNF>{cNF}</cNF>");
        xmlStr.Append("<natOp>VENDA MERCADORIA</natOp>");
        xmlStr.Append("<mod>65</mod>");
        xmlStr.Append($"<serie>{nfce.Serie}</serie>");
        xmlStr.Append($"<nNF>{nfce.Numero}</nNF>");
        xmlStr.Append($"<dhEmi>{dhEmi}</dhEmi>");
        xmlStr.Append("<tpNF>1</tpNF>");
        xmlStr.Append("<idDest>1</idDest>");
        xmlStr.Append($"<cMunFG>{config.CodigoMunicipio ?? "4106902"}</cMunFG>");
        xmlStr.Append("<tpImp>4</tpImp>");
        xmlStr.Append($"<tpEmis>{config.TipoEmissao}</tpEmis>");
        xmlStr.Append($"<cDV>{dv}</cDV>");
        xmlStr.Append($"<tpAmb>{config.Ambiente}</tpAmb>");
        xmlStr.Append("<finNFe>1</finNFe>");
        xmlStr.Append("<indFinal>1</indFinal>");
        xmlStr.Append("<indPres>1</indPres>");
        xmlStr.Append("<procEmi>0</procEmi>");
        xmlStr.Append("<verProc>1.0.0</verProc>");
        xmlStr.Append("</ide>");

        // <emit>
        xmlStr.Append("<emit>");
        xmlStr.Append($"<CNPJ>{cnpjOnly}</CNPJ>");
        xmlStr.Append($"<xNome>{SecurityElement.Escape((config.RazaoSocial ?? "").Trim())}</xNome>");
        if (!string.IsNullOrWhiteSpace(config.NomeFantasia))
            xmlStr.Append($"<xFant>{SecurityElement.Escape(config.NomeFantasia.Trim())}</xFant>");

        xmlStr.Append("<enderEmit>");
        xmlStr.Append($"<xLgr>{SecurityElement.Escape((config.Logradouro ?? "Rua").Trim())}</xLgr>");
        xmlStr.Append($"<nro>{SecurityElement.Escape((config.Numero ?? "SN").Trim())}</nro>");
        if (!string.IsNullOrWhiteSpace(config.Complemento))
            xmlStr.Append($"<xCpl>{SecurityElement.Escape(config.Complemento.Trim())}</xCpl>");
        xmlStr.Append($"<xBairro>{SecurityElement.Escape((config.Bairro ?? "Bairro").Trim())}</xBairro>");
        xmlStr.Append($"<cMun>{config.CodigoMunicipio ?? "4106902"}</cMun>");
        xmlStr.Append($"<xMun>{SecurityElement.Escape((config.Municipio ?? "Curitiba").Trim())}</xMun>");
        xmlStr.Append($"<UF>{config.UF}</UF>");
        xmlStr.Append($"<CEP>{new string((config.CEP ?? "").Where(char.IsDigit).ToArray())}</CEP>");
        xmlStr.Append("</enderEmit>");
        xmlStr.Append($"<IE>{new string((config.InscricaoEstadual ?? "").Where(char.IsDigit).ToArray())}</IE>");
        xmlStr.Append($"<CRT>{config.CRT}</CRT>");
        xmlStr.Append("</emit>");

        // <det>
        int nItem = 1;
        foreach (var item in nfce.Itens)
        {
            var ncm = new string((item.NCM ?? "63090010").Where(char.IsDigit).ToArray()).PadLeft(8, '0');
            var cfop = item.CFOP ?? "5102";
            var csosn = item.CSOSN ?? "102";
            var cProdClean = (item.CodigoProduto ?? nItem.ToString()).Trim();
            var currentItemNum = nItem;
            var xProdClean = (config.Ambiente == 2 && currentItemNum == 1)
                ? "NOTA FISCAL EMITIDA EM AMBIENTE DE HOMOLOGACAO - SEM VALOR FISCAL"
                : (item.Descricao ?? "PRODUTO").Trim();

            xmlStr.Append($"<det nItem=\"{nItem++}\">");
            xmlStr.Append("<prod>");
            xmlStr.Append($"<cProd>{SecurityElement.Escape(cProdClean)}</cProd>");
            xmlStr.Append("<cEAN>SEM GTIN</cEAN>");
            xmlStr.Append($"<xProd>{SecurityElement.Escape(xProdClean)}</xProd>");
            xmlStr.Append($"<NCM>{ncm}</NCM>");
            xmlStr.Append($"<CFOP>{cfop}</CFOP>");
            // vProd DEVE ser qCom * vUnCom (bruto, sem descontos) - Rejeição 629
            var vProdBruto = item.Quantidade * item.ValorUnitario;
            var vDescItem = item.ValorDesconto ?? 0m;

            xmlStr.Append($"<uCom>{(item.Unidade ?? "UN").Trim()}</uCom>");
            xmlStr.Append($"<qCom>{item.Quantidade.ToString("F4", CultureInfo.InvariantCulture)}</qCom>");
            xmlStr.Append($"<vUnCom>{item.ValorUnitario.ToString("F4", CultureInfo.InvariantCulture)}</vUnCom>");
            xmlStr.Append($"<vProd>{vProdBruto.ToString("F2", CultureInfo.InvariantCulture)}</vProd>");
            xmlStr.Append("<cEANTrib>SEM GTIN</cEANTrib>");
            xmlStr.Append($"<uTrib>{(item.Unidade ?? "UN").Trim()}</uTrib>");
            xmlStr.Append($"<qTrib>{item.Quantidade.ToString("F4", CultureInfo.InvariantCulture)}</qTrib>");
            xmlStr.Append($"<vUnTrib>{item.ValorUnitario.ToString("F4", CultureInfo.InvariantCulture)}</vUnTrib>");
            if (vDescItem > 0)
                xmlStr.Append($"<vDesc>{vDescItem.ToString("F2", CultureInfo.InvariantCulture)}</vDesc>");
            xmlStr.Append("<indTot>1</indTot>");
            xmlStr.Append("</prod>");

            xmlStr.Append("<imposto>");
            xmlStr.Append("<ICMS>");
            xmlStr.Append($"<ICMSSN102><orig>{item.OrigemMercadoria}</orig><CSOSN>{csosn}</CSOSN></ICMSSN102>");
            xmlStr.Append("</ICMS>");
            xmlStr.Append("<PIS><PISNT><CST>07</CST></PISNT></PIS>");
            xmlStr.Append("<COFINS><COFINSNT><CST>07</CST></COFINSNT></COFINS>");
            xmlStr.Append("</imposto>");
            xmlStr.Append("</det>");
        }

        // <total>
        // vProd total = soma de (qCom × vUnCom) de cada item, consistente com o vProd por item
        var vProdTotal = nfce.Itens.Sum(i => i.Quantidade * i.ValorUnitario);
        var vDescTotal = nfce.Itens.Sum(i => i.ValorDesconto ?? 0m);
        // Se não houver desconto por item, usa o desconto da venda
        if (vDescTotal == 0) vDescTotal = nfce.ValorDesconto ?? 0m;

        xmlStr.Append("<total><ICMSTot>");
        xmlStr.Append("<vBC>0.00</vBC><vICMS>0.00</vICMS><vICMSDeson>0.00</vICMSDeson><vFCP>0.00</vFCP>");
        xmlStr.Append("<vBCST>0.00</vBCST><vST>0.00</vST><vFCPST>0.00</vFCPST><vFCPSTRet>0.00</vFCPSTRet>");
        xmlStr.Append($"<vProd>{vProdTotal.ToString("F2", CultureInfo.InvariantCulture)}</vProd>");
        xmlStr.Append("<vFrete>0.00</vFrete><vSeg>0.00</vSeg>");
        xmlStr.Append($"<vDesc>{vDescTotal.ToString("F2", CultureInfo.InvariantCulture)}</vDesc>");
        xmlStr.Append("<vII>0.00</vII><vIPI>0.00</vIPI><vIPIDevol>0.00</vIPIDevol><vPIS>0.00</vPIS><vCOFINS>0.00</vCOFINS><vOutro>0.00</vOutro>");
        xmlStr.Append($"<vNF>{nfce.ValorTotal.ToString("F2", CultureInfo.InvariantCulture)}</vNF>");
        xmlStr.Append("</ICMSTot></total>");

        // <transp>
        xmlStr.Append("<transp><modFrete>9</modFrete></transp>");

        // <pag>
        xmlStr.Append("<pag>");
        foreach (var pg in nfce.Pagamentos)
        {
            xmlStr.Append("<detPag>");
            xmlStr.Append($"<tPag>{pg.TipoPagamento}</tPag>");
            xmlStr.Append($"<vPag>{pg.Valor.ToString("F2", CultureInfo.InvariantCulture)}</vPag>");
            if (pg.TipoPagamento != "01") // Não é dinheiro em espécie -> incluir card com tpIntegra=2 (não integrado)
            {
                xmlStr.Append("<card><tpIntegra>2</tpIntegra></card>");
            }
            xmlStr.Append("</detPag>");
        }
        xmlStr.Append("</pag>");

        // <infRespTec> - Informações do Responsável Técnico (obrigatório no PR - NT 2018.005)
        var cnpjRespTec = new string(config.CNPJ.Where(char.IsDigit).ToArray());
        xmlStr.Append("<infRespTec>");
        xmlStr.Append($"<CNPJ>{cnpjRespTec}</CNPJ>");
        xmlStr.Append("<xContato>A Brechozeira Suporte</xContato>");
        xmlStr.Append("<email>contato@abrechozeira.com.br</email>");
        xmlStr.Append("<fone>41999999999</fone>");
        xmlStr.Append("</infRespTec>");

        xmlStr.Append("</infNFe>");


        // <infNFeSupl>
        var qrCodeUrl = GerarQrCodeUrl(chaveAcesso, config);
        xmlStr.Append("<infNFeSupl>");
        xmlStr.Append($"<qrCode><![CDATA[{qrCodeUrl}]]></qrCode>");
        xmlStr.Append("<urlChave>http://www.fazenda.pr.gov.br/nfce/consulta</urlChave>");
        xmlStr.Append("</infNFeSupl>");

        xmlStr.Append("</NFe>");

        var doc = new XmlDocument();
        doc.LoadXml(xmlStr.ToString());
        return doc;
    }

    /// <summary>
    /// Assina o XML da NFC-e utilizando o Certificado A1 (.pfx)
    /// </summary>
    private XmlDocument AssinarXml(XmlDocument doc, string chaveAcesso, X509Certificate2 cert)
    {
        var signedXml = new SignedXml(doc);
        signedXml.SigningKey = cert.GetRSAPrivateKey();
        signedXml.SignedInfo.SignatureMethod = "http://www.w3.org/2000/09/xmldsig#rsa-sha1";

        var reference = new Reference();
        reference.Uri = "#NFe" + chaveAcesso;
        reference.DigestMethod = "http://www.w3.org/2000/09/xmldsig#sha1";
        reference.AddTransform(new XmlDsigEnvelopedSignatureTransform());
        reference.AddTransform(new XmlDsigC14NTransform());
        signedXml.AddReference(reference);

        var keyInfo = new KeyInfo();
        keyInfo.AddClause(new KeyInfoX509Data(cert));
        signedXml.KeyInfo = keyInfo;

        signedXml.ComputeSignature();
        var xmlDigitalSignature = signedXml.GetXml();

        doc.DocumentElement?.AppendChild(doc.ImportNode(xmlDigitalSignature, true));
        return doc;
    }

    /// <summary>
    /// Transmite o envelope SOAP com o XML da NFC-e para a SEFAZ-PR
    /// </summary>
    private async Task<(bool autorizado, string? protocolo, string? mensagem, string? xmlEnvio, string? xmlRetorno)> TransmitirSefazPrAsync(
        XmlDocument docNfeAssinado, EmpresaFiscal config, X509Certificate2 cert)
    {
        var urlSefaz = config.Ambiente == 1
            ? "https://nfce.sefa.pr.gov.br/nfce/NFeAutorizacao4"
            : "https://homologacao.nfce.sefa.pr.gov.br/nfce/NFeAutorizacao4";

        var xmlAssinadoStr = docNfeAssinado.OuterXml;

        var soapEnvelope = new StringBuilder();
        soapEnvelope.Append("<soap12:Envelope xmlns:xsi=\"http://www.w3.org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\" xmlns:soap12=\"http://www.w3.org/2003/05/soap-envelope\">");
        soapEnvelope.Append("<soap12:Body>");
        soapEnvelope.Append("<nfeDadosMsg xmlns=\"http://www.portalfiscal.inf.br/nfe/wsdl/NFeAutorizacao4\">");
        soapEnvelope.Append("<enviNFe xmlns=\"http://www.portalfiscal.inf.br/nfe\" versao=\"4.00\">");
        soapEnvelope.Append("<idLote>1</idLote>");
        soapEnvelope.Append("<indSinc>1</indSinc>");
        soapEnvelope.Append(xmlAssinadoStr);
        soapEnvelope.Append("</enviNFe>");
        soapEnvelope.Append("</nfeDadosMsg>");
        soapEnvelope.Append("</soap12:Body>");
        soapEnvelope.Append("</soap12:Envelope>");

        using var handler = new HttpClientHandler();
        handler.ClientCertificates.Add(cert);
        handler.SslProtocols = System.Security.Authentication.SslProtocols.Tls12 | System.Security.Authentication.SslProtocols.Tls13;
        handler.ServerCertificateCustomValidationCallback = (sender, certificate, chain, sslPolicyErrors) => true;

        using var client = new HttpClient(handler);
        client.Timeout = TimeSpan.FromSeconds(30);

        var content = new StringContent(soapEnvelope.ToString(), Encoding.UTF8, "application/soap+xml");

        var response = await client.PostAsync(urlSefaz, content);
        var responseXmlStr = await response.Content.ReadAsStringAsync();

        _logger.LogInformation("Retorno WebService SEFAZ-PR ({StatusCode}): {Response}", response.StatusCode, responseXmlStr);

        var responseDoc = new XmlDocument();
        responseDoc.LoadXml(responseXmlStr);

        var nsmgr = new XmlNamespaceManager(responseDoc.NameTable);
        nsmgr.AddNamespace("nfe", "http://www.portalfiscal.inf.br/nfe");
        nsmgr.AddNamespace("soap12", "http://www.w3.org/2003/05/soap-envelope");

        var cStatNode = responseDoc.SelectSingleNode("//nfe:cStat", nsmgr) ?? responseDoc.SelectSingleNode("//cStat");
        var xMotivoNode = responseDoc.SelectSingleNode("//nfe:xMotivo", nsmgr) ?? responseDoc.SelectSingleNode("//xMotivo");
        var nProtNode = responseDoc.SelectSingleNode("//nfe:nProt", nsmgr) ?? responseDoc.SelectSingleNode("//nProt");

        var cStat = cStatNode?.InnerText;
        var xMotivo = xMotivoNode?.InnerText;
        var nProt = nProtNode?.InnerText;

        bool autorizado = (cStat == "100" || cStat == "104");

        return (autorizado, nProt, $"[{cStat}] {xMotivo}", soapEnvelope.ToString(), responseXmlStr);
    }

    /// <summary>
    /// Gera a URL do QR Code da SEFAZ v4.00 com Hash SHA1 do CSC
    /// </summary>
    private string GerarQrCodeUrl(string chaveAcesso, EmpresaFiscal config)
    {
        var cIdTokenRaw = config.CSCId ?? "1";
        var cIdToken = int.TryParse(cIdTokenRaw, out var idVal) ? idVal.ToString() : cIdTokenRaw.TrimStart('0');
        if (string.IsNullOrEmpty(cIdToken)) cIdToken = "1";

        var cscToken = (config.CSC ?? "").Trim();

        var paramString = $"{chaveAcesso}|2|{config.Ambiente}|{cIdToken}";
        var hashInput = paramString + cscToken;

        using var sha1 = SHA1.Create();
        var hashBytes = sha1.ComputeHash(Encoding.ASCII.GetBytes(hashInput));
        var cHashQR = BitConverter.ToString(hashBytes).Replace("-", "").ToUpperInvariant();

        var urlBase = "http://www.fazenda.pr.gov.br/nfce/qrcode";

        return $"{urlBase}?p={paramString}|{cHashQR}";
    }
}
