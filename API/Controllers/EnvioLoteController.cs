using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using ABrechozeiraApp.Models;
using ABrechozeiraApp.Services;
using System.Text.RegularExpressions;
using System.Text.Json;

namespace ABrechozeiraApp.Controllers
{
    // ─── DTOs ────────────────────────────────────────────────────────────────────

    public class ParseTextoInput
    {
        public string Texto { get; set; } = "";
    }

    public class EnvioParseado
    {
        public int Indice { get; set; }
        public string Nome { get; set; } = "";
        public decimal? Valor { get; set; }
        public int PesoGramas { get; set; }
        public decimal Altura { get; set; }
        public decimal Largura { get; set; }
        public decimal Comprimento { get; set; }
        // Dados do cliente encontrado no sistema
        public bool ClienteEncontrado { get; set; }
        public int? ClienteId { get; set; }
        public string? ClienteNomeSistema { get; set; }
        public int? EnderecoId { get; set; }
        public string? CepDestino { get; set; }
        public string? EnderecoCompleto { get; set; }
        
        public string? DestinatarioEndereco { get; set; }
        public string? DestinatarioNumero { get; set; }
        public string? DestinatarioBairro { get; set; }
        public string? DestinatarioCidade { get; set; }
        public string? DestinatarioEstado { get; set; }
        public string? DestinatarioCpf { get; set; }
        public string? DestinatarioEmail { get; set; }
        public string? DestinatarioComplemento { get; set; }
        // Validação
        public bool Valido { get; set; } = true;
        public List<string> Erros { get; set; } = new();
    }

    public class ParseTextoResultado
    {
        public List<EnvioParseado> Envios { get; set; } = new();
        public List<EnvioParseado> NaoProcessados { get; set; } = new();
        public int TotalParseados { get; set; }
        public int TotalValidos { get; set; }
        public int TotalInvalidos { get; set; }
    }

    public class CotarEGerarInput
    {
        public List<EnvioParaCotar> Envios { get; set; } = new();
    }

    public class EnvioParaCotar
    {
        public int Indice { get; set; }
        public string Nome { get; set; } = "";
        public decimal Valor { get; set; }
        public int PesoGramas { get; set; }
        public decimal Altura { get; set; }
        public decimal Largura { get; set; }
        public decimal Comprimento { get; set; }
        public int? ClienteId { get; set; }
        public int? EnderecoId { get; set; }
        public string CepDestino { get; set; } = "";
        // Dados do destinatário para a etiqueta
        public string DestinatarioEndereco { get; set; } = "";
        public string DestinatarioNumero { get; set; } = "";
        public string DestinatarioBairro { get; set; } = "";
        public string DestinatarioCidade { get; set; } = "";
        public string DestinatarioEstado { get; set; } = "";
        public string? DestinatarioComplemento { get; set; }
        public string? DestinatarioEmail { get; set; }
        public string? DestinatarioCpf { get; set; }
        public string PreferenciaServico { get; set; } = "AUTO"; // AUTO, PAC, SEDEX
    }

    public class EnvioLoteResultado
    {
        public int Indice { get; set; }
        public string Nome { get; set; } = "";
        public decimal Valor { get; set; }
        // Cotação
        public string ServicoEscolhido { get; set; } = "";
        public int ServicoId { get; set; }
        public decimal PrecoPAC { get; set; }
        public decimal? PrecoSEDEX { get; set; }
        public decimal PrecoEscolhido { get; set; }
        public string MotivoEscolha { get; set; } = "";
        // Etiqueta
        public bool EtiquetaGerada { get; set; }
        public string? EtiquetaId { get; set; }
        public string? EtiquetaStatus { get; set; }
        public decimal? EtiquetaPreco { get; set; }
        // Erros
        public bool Sucesso { get; set; }
        public string? Erro { get; set; }
    }

    public class CotarEGerarResultado
    {
        public List<EnvioLoteResultado> Resultados { get; set; } = new();
        public int TotalSucesso { get; set; }
        public int TotalErro { get; set; }
        public decimal CustoTotal { get; set; }
    }

    // ─── DTOs para operações separadas ────────────────────────────────────────

    public class CotarLoteInput
    {
        public List<EnvioParaCotar> Envios { get; set; } = new();
    }

    public class CotacaoLoteItem
    {
        public int Indice { get; set; }
        public string Nome { get; set; } = "";
        public decimal PrecoPAC { get; set; }
        public decimal? PrecoSEDEX { get; set; }
        public string ServicoIdRecomendado { get; set; } = "";
        public string ServicoRecomendado { get; set; } = "";
        public decimal PrecoRecomendado { get; set; }
        public string MotivoEscolha { get; set; } = "";
        public string TransacaoId { get; set; } = "";
        // Canais disponíveis e resultado do envio automático da cotação ao cliente
        public bool TemEmail { get; set; }
        public bool TemWhatsApp { get; set; }
        public bool EmailEnviado { get; set; }
        public bool WhatsAppEnviado { get; set; }
        public List<string> ErrosEnvio { get; set; } = new();
        public bool Sucesso { get; set; }
        public string? Erro { get; set; }
    }

    public class CotarLoteResultado
    {
        public List<CotacaoLoteItem> Resultados { get; set; } = new();
        public int TotalSucesso { get; set; }
        public int TotalErro { get; set; }
    }

    public class GerarEtiquetasLoteInput
    {
        public List<EnvioParaGerarEtiqueta> Envios { get; set; } = new();
    }

    public class EnvioParaGerarEtiqueta : EnvioParaCotar
    {
        public string ServicoId { get; set; } = "";
        public string TransacaoId { get; set; } = "";
    }

    public class EtiquetaLoteItem
    {
        public int Indice { get; set; }
        public string Nome { get; set; } = "";
        public bool Sucesso { get; set; }
        public string? EtiquetaId { get; set; }
        public string? EtiquetaStatus { get; set; }
        public decimal? EtiquetaPreco { get; set; }
        public string? Erro { get; set; }
    }

    public class PaymentTagMapInfo
    {
        public string EtiquetaId { get; set; } = "";
        public string Email { get; set; } = "";
        public string Nome { get; set; } = "";
        public string StatusPagamento { get; set; } = "Aguardando";
        public string StatusSuperfrete { get; set; } = "Carrinho";
        public bool EmailCotacaoEnviado { get; set; }
        public bool EmailRastreioEnviado { get; set; }
        public bool WhatsAppCotacaoEnviado { get; set; }
        public bool WhatsAppRastreioEnviado { get; set; }
        public string? OrigemPagamento { get; set; }
        public DateTime? PagoEm { get; set; }
        public string? PagoManualPor { get; set; }
    }

    public class GerarEtiquetasLoteResultado
    {
        public List<EtiquetaLoteItem> Resultados { get; set; } = new();
        public int TotalSucesso { get; set; }
        public int TotalErro { get; set; }
        public decimal CustoTotal { get; set; }
    }

    public class EnviarRastreioLoteInput
    {
        public List<EnviarRastreioLoteItemInput>? Envios { get; set; }
    }

    public class EnviarRastreioLoteItemInput
    {
        public string? EtiquetaId { get; set; }
        public string? Email { get; set; }
        public string? Nome { get; set; }
    }

    public class EnviarRastreioLoteResultado
    {
        public int TotalSucesso { get; set; }
        public int TotalErro { get; set; }
        public List<RastreioLoteItemErro>? Erros { get; set; }
    }

    public class RastreioLoteItemErro
    {
        public string? Nome { get; set; }
        public string? Erro { get; set; }
    }

    // ─── Controller ──────────────────────────────────────────────────────────
    [ApiController]
    [Route("api/[controller]")]
    public class EnvioLoteController : ControllerBase
    {
        private readonly EmailService _emailService;
        private readonly SuperfreteService _superfrete;
        private readonly InfinitePayService _infinitePay;
        private readonly ILogger<EnvioLoteController> _logger;
        private readonly AbrechozeiraContext _context;
        private readonly IConfiguration _config;
        private readonly WhatsAppService _whatsApp;
        private readonly PagamentoFreteService _pagamentoFrete;

        public EnvioLoteController(
            EmailService emailService,
            SuperfreteService superfrete,
            InfinitePayService infinitePay,
            ILogger<EnvioLoteController> logger,
            AbrechozeiraContext context,
            IConfiguration config,
            WhatsAppService whatsApp,
            PagamentoFreteService pagamentoFrete)
        {
            _emailService = emailService;
            _superfrete = superfrete;
            _infinitePay = infinitePay;
            _logger = logger;
            _context = context;
            _config = config;
            _whatsApp = whatsApp;
            _pagamentoFrete = pagamentoFrete;
        }

        /// <summary>
        /// Recebe o texto colado do WhatsApp e faz o parsing automático,
        /// retornando uma lista estruturada de envios com validações.
        /// </summary>
        [HttpPost("ParseTexto")]
        public async Task<IActionResult> ParseTexto([FromBody] ParseTextoInput input)
        {
            if (string.IsNullOrWhiteSpace(input.Texto))
                return BadRequest(new { message = "Texto vazio." });

            var envios = ParseWhatsAppText(input.Texto);
            var validos = new List<EnvioParseado>();
            var invalidos = new List<EnvioParseado>();

            foreach (var envio in envios)
            {
                // Validar dados obrigatórios
                if (!envio.Valor.HasValue || envio.Valor <= 0)
                {
                    envio.Valido = false;
                    envio.Erros.Add("Valor não informado");
                }
                if (envio.PesoGramas <= 0)
                {
                    envio.Valido = false;
                    envio.Erros.Add("Peso não informado");
                }
                if (envio.Altura <= 0 || envio.Largura <= 0 || envio.Comprimento <= 0)
                {
                    envio.Valido = false;
                    envio.Erros.Add("Dimensões incompletas");
                }
                if (string.IsNullOrWhiteSpace(envio.Nome))
                {
                    envio.Valido = false;
                    envio.Erros.Add("Nome não identificado");
                }

                // Buscar cliente no sistema por nome (apenas se válido)
                if (!string.IsNullOrWhiteSpace(envio.Nome))
                {
                    await BuscarClientePorNome(envio);
                }

                if (envio.Valido)
                    validos.Add(envio);
                else
                    invalidos.Add(envio);
            }

            var resultado = new ParseTextoResultado
            {
                Envios = validos,
                NaoProcessados = invalidos,
                TotalParseados = envios.Count,
                TotalValidos = validos.Count,
                TotalInvalidos = invalidos.Count
            };

            return Ok(resultado);
        }

        /// <summary>
        /// Cota o frete (PAC e SEDEX) e gera etiquetas em lote.
        /// Regra: Usa PAC como padrão. Usa SEDEX se for mais barato OU no máximo R$3 acima do PAC.
        /// </summary>
        [HttpPost("CotarEGerarLote")]
        public async Task<IActionResult> CotarEGerarLote([FromBody] CotarEGerarInput input)
        {
            if (input.Envios == null || input.Envios.Count == 0)
                return BadRequest(new { message = "Nenhum envio para processar." });

            var resultados = new List<EnvioLoteResultado>();
            var cepOrigem = (_config["Superfrete:Remetente:CEP"] ?? "").Replace("-", "");

            foreach (var envio in input.Envios)
            {
                var resultado = new EnvioLoteResultado
                {
                    Indice = envio.Indice,
                    Nome = envio.Nome,
                    Valor = envio.Valor
                };

                try
                {
                    if (string.IsNullOrWhiteSpace(envio.CepDestino))
                    {
                        resultado.Sucesso = false;
                        resultado.Erro = "CEP de destino não informado";
                        resultados.Add(resultado);
                        continue;
                    }

                    // 1. Cotar frete (PAC = 1, SEDEX = 2)
                    var pesoKg = envio.PesoGramas / 1000m;
                    if (pesoKg < 0.3m) pesoKg = 0.3m; // Peso mínimo 300g

                    var cotacaoRequest = new CotacaoFreteRequest
                    {
                        From = new CotacaoEndereco { PostalCode = cepOrigem },
                        To = new CotacaoEndereco { PostalCode = envio.CepDestino.Replace("-", "") },
                        Services = "1,2", // PAC e SEDEX
                        Options = new CotacaoOpcoes
                        {
                            InsuranceValue = envio.Valor,
                            UseInsuranceValue = envio.Valor > 0
                        },
                        Package = new CotacaoPacote
                        {
                            Weight = pesoKg,
                            Height = envio.Altura,
                            Width = envio.Largura,
                            Length = envio.Comprimento
                        }
                    };

                    var cotacoes = await _superfrete.CotarFreteAsync(cotacaoRequest);
                    var pac = cotacoes.FirstOrDefault(c => c.Id == 1 && !c.HasError);
                    var sedex = cotacoes.FirstOrDefault(c => c.Id == 2 && !c.HasError);

                    if (pac == null && sedex == null)
                    {
                        resultado.Sucesso = false;
                        resultado.Erro = "Nenhuma cotação disponível para este destino";
                        resultados.Add(resultado);
                        continue;
                    }

                    // 2. Aplicar regra de seleção PAC/SEDEX
                    int servicoId;
                    string servicoNome;
                    decimal precoEscolhido;
                    string motivoEscolha;

                    if (pac != null)
                    {
                        resultado.PrecoPAC = pac.Price;
                    }
                    if (sedex != null)
                    {
                        resultado.PrecoSEDEX = sedex.Price;
                    }

                    if (pac == null && sedex != null)
                    {
                        // Só SEDEX disponível
                        servicoId = 2;
                        servicoNome = "SEDEX";
                        precoEscolhido = sedex.Price;
                        motivoEscolha = "PAC indisponível, usado SEDEX";
                    }
                    else if (sedex != null && sedex.Price <= pac!.Price)
                    {
                        // SEDEX mais barato ou igual ao PAC
                        servicoId = 2;
                        servicoNome = "SEDEX";
                        precoEscolhido = sedex.Price;
                        motivoEscolha = $"SEDEX mais barato que PAC (SEDEX R${sedex.Price:F2} vs PAC R${pac.Price:F2})";
                    }
                    else if (sedex != null && (sedex.Price - pac!.Price) <= 3.0m)
                    {
                        // SEDEX no máximo R$3 acima do PAC
                        servicoId = 2;
                        servicoNome = "SEDEX";
                        precoEscolhido = sedex.Price;
                        motivoEscolha = $"SEDEX apenas R${(sedex.Price - pac.Price):F2} acima do PAC (SEDEX R${sedex.Price:F2} vs PAC R${pac.Price:F2})";
                    }
                    else
                    {
                        // PAC padrão
                        servicoId = 1;
                        servicoNome = "PAC";
                        precoEscolhido = pac!.Price;
                        motivoEscolha = sedex != null
                            ? $"PAC escolhido (SEDEX R${sedex.Price:F2} é R${(sedex.Price - pac.Price):F2} acima do PAC)"
                            : "PAC escolhido (SEDEX indisponível)";
                    }

                    resultado.ServicoId = servicoId;
                    resultado.ServicoEscolhido = servicoNome;
                    resultado.PrecoEscolhido = precoEscolhido;
                    resultado.MotivoEscolha = motivoEscolha;

                    // 3. Gerar etiqueta
                    var etiquetaRequest = new CriarEtiquetaRequest
                    {
                        Service = servicoId,
                        Platform = "Abrechozeira",
                        From = new EtiquetaEndereco(), // Será preenchido pelo service com dados do appsettings
                        To = new EtiquetaDestinatario
                        {
                            Name = envio.Nome,
                            Address = envio.DestinatarioEndereco,
                            Number = envio.DestinatarioNumero,
                            Complement = envio.DestinatarioComplemento,
                            District = envio.DestinatarioBairro,
                            City = envio.DestinatarioCidade,
                            StateAbbr = envio.DestinatarioEstado,
                            PostalCode = envio.CepDestino.Replace("-", ""),
                            Email = envio.DestinatarioEmail,
                            Document = envio.DestinatarioCpf
                        },
                        Volumes = new CotacaoPacote
                        {
                            Weight = pesoKg,
                            Height = envio.Altura,
                            Width = envio.Largura,
                            Length = envio.Comprimento
                        },
                        Products = new List<EtiquetaProduto>
                        {
                            new EtiquetaProduto
                            {
                                Name = "Produtos Brechó",
                                Quantity = 1,
                                UnitaryValue = envio.Valor
                            }
                        },
                        Options = new EtiquetaOpcoes
                        {
                            InsuranceValue = envio.Valor,
                            NonCommercial = true
                        }
                    };

                    var etiqueta = await _superfrete.CriarEtiquetaAsync(etiquetaRequest);
                    resultado.EtiquetaGerada = true;
                    resultado.EtiquetaId = etiqueta.Id;
                    resultado.EtiquetaStatus = etiqueta.Status;
                    resultado.EtiquetaPreco = etiqueta.Price;
                    resultado.Sucesso = true;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao processar envio em lote para {Nome}", envio.Nome);
                    resultado.Sucesso = false;
                    resultado.Erro = ex.Message;
                }

                resultados.Add(resultado);
            }

            var retorno = new CotarEGerarResultado
            {
                Resultados = resultados,
                TotalSucesso = resultados.Count(r => r.Sucesso),
                TotalErro = resultados.Count(r => !r.Sucesso),
                CustoTotal = resultados.Where(r => r.Sucesso && r.EtiquetaPreco.HasValue).Sum(r => r.EtiquetaPreco!.Value)
            };

            return Ok(retorno);
        }

        /// <summary>
        /// Cota o frete (PAC e SEDEX) SEM gerar etiqueta.
        /// Retorna preços e serviço recomendado pela regra PAC/SEDEX.
        /// </summary>
        [HttpPost("CotarLote")]
        public async Task<IActionResult> CotarLote([FromBody] CotarLoteInput input)
        {
            if (input.Envios == null || input.Envios.Count == 0)
                return BadRequest(new { message = "Nenhum envio para cotar." });

            var resultados = new List<CotacaoLoteItem>();
            var cepOrigem = (_config["Superfrete:Remetente:CEP"] ?? "").Replace("-", "");

            foreach (var envio in input.Envios)
            {
                var transacaoId = Guid.NewGuid().ToString("N");
                var resultado = new CotacaoLoteItem
                {
                    Indice = envio.Indice,
                    Nome = envio.Nome,
                    TransacaoId = transacaoId
                };

                try
                {
                    if (string.IsNullOrWhiteSpace(envio.CepDestino))
                    {
                        resultado.Sucesso = false;
                        resultado.Erro = "CEP de destino não informado";
                        resultados.Add(resultado);
                        continue;
                    }

                    var pesoKg = envio.PesoGramas / 1000m;
                    if (pesoKg < 0.3m) pesoKg = 0.3m;

                    var cotacaoRequest = new CotacaoFreteRequest
                    {
                        From = new CotacaoEndereco { PostalCode = cepOrigem },
                        To = new CotacaoEndereco { PostalCode = envio.CepDestino.Replace("-", "") },
                        Services = "1,2",
                        Options = new CotacaoOpcoes
                        {
                            InsuranceValue = envio.Valor,
                            UseInsuranceValue = envio.Valor > 0
                        },
                        Package = new CotacaoPacote
                        {
                            Weight = pesoKg,
                            Height = envio.Altura,
                            Width = envio.Largura,
                            Length = envio.Comprimento
                        }
                    };

                    var cotacoes = await _superfrete.CotarFreteAsync(cotacaoRequest);
                    var pac = cotacoes.FirstOrDefault(c => c.Id == 1 && !c.HasError);
                    var sedex = cotacoes.FirstOrDefault(c => c.Id == 2 && !c.HasError);

                    if (pac == null && sedex == null)
                    {
                        resultado.Sucesso = false;
                        resultado.Erro = "Nenhuma cotação disponível";
                        resultados.Add(resultado);
                        continue;
                    }

                    if (pac != null) resultado.PrecoPAC = pac.Price;
                    if (sedex != null) resultado.PrecoSEDEX = sedex.Price;

                    // Aplicar regra PAC/SEDEX ou Preferência do Usuário
                    var pref = envio.PreferenciaServico?.ToUpper() ?? "AUTO";

                    if (pref == "PAC" && pac != null)
                    {
                        resultado.ServicoIdRecomendado = "1";
                        resultado.ServicoRecomendado = "PAC";
                        resultado.PrecoRecomendado = pac.Price;
                        resultado.MotivoEscolha = "PAC selecionado pelo usuário";
                    }
                    else if (pref == "SEDEX" && sedex != null)
                    {
                        resultado.ServicoIdRecomendado = "2";
                        resultado.ServicoRecomendado = "SEDEX";
                        resultado.PrecoRecomendado = sedex.Price;
                        resultado.MotivoEscolha = "SEDEX selecionado pelo usuário";
                    }
                    else if (pac == null && sedex != null)
                    {
                        resultado.ServicoIdRecomendado = "2";
                        resultado.ServicoRecomendado = "SEDEX";
                        resultado.PrecoRecomendado = sedex.Price;
                        resultado.MotivoEscolha = pref != "AUTO" ? $"SEDEX (preferência {pref} indisponível)" : "PAC indisponível";
                    }
                    else if (sedex != null && sedex.Price <= pac!.Price)
                    {
                        resultado.ServicoIdRecomendado = "2";
                        resultado.ServicoRecomendado = "SEDEX";
                        resultado.PrecoRecomendado = sedex.Price;
                        resultado.MotivoEscolha = $"SEDEX mais barato (R${sedex.Price:F2} vs PAC R${pac.Price:F2})";
                    }
                    else if (sedex != null && (sedex.Price - pac!.Price) <= 3.0m)
                    {
                        resultado.ServicoIdRecomendado = "2";
                        resultado.ServicoRecomendado = "SEDEX";
                        resultado.PrecoRecomendado = sedex.Price;
                        resultado.MotivoEscolha = $"SEDEX +R${(sedex.Price - pac.Price):F2} do PAC";
                    }
                    else
                    {
                        resultado.ServicoIdRecomendado = "1";
                        resultado.ServicoRecomendado = "PAC";
                        resultado.PrecoRecomendado = pac!.Price;
                        resultado.MotivoEscolha = sedex != null
                            ? (pref != "AUTO" ? $"PAC (preferência {pref} indisponível)" : $"SEDEX R${(sedex.Price - pac.Price):F2} acima")
                            : (pref != "AUTO" ? $"PAC (preferência {pref} indisponível)" : "SEDEX indisponível");
                    }

                    // Adicionar repasse de custo do WhatsApp
                    var msgCost = _config.GetValue<decimal>("WhatsApp:MessageCost", 0m);
                    var markup = _config.GetValue<decimal>("WhatsApp:MarkupPercentage", 100m);
                    var repasseZap = msgCost + (msgCost * (markup / 100m));
                    resultado.PrecoRecomendado += repasseZap;

                    resultado.Sucesso = true;

                    // Salvar mapeamento parcial (sem link de checkout ainda)
                    var map = new EnvioLoteMap
                    {
                        TransacaoId = transacaoId,
                        Nome = envio.Nome,
                        Email = envio.DestinatarioEmail ?? "",
                        StatusPagamento = "Aguardando",
                        StatusSuperfrete = "Carrinho",
                        PrecoPAC = resultado.PrecoPAC,
                        PrecoSEDEX = resultado.PrecoSEDEX,
                        PrecoRecomendado = resultado.PrecoRecomendado,
                        ServicoRecomendado = resultado.ServicoRecomendado,
                        CreatedAt = DateTime.Now
                    };
                    _context.EnvioLoteMap.Add(map);
                    await _context.SaveChangesAsync();

                    // Envia a cotação ao cliente automaticamente por e-mail e WhatsApp
                    var envioCliente = await EnviarCotacaoAoClienteAsync(map, envio.ClienteId, viaEmail: true, viaWhatsApp: true);
                    resultado.TemEmail = !envioCliente.SemEmail;
                    resultado.TemWhatsApp = !envioCliente.SemWhatsApp;
                    resultado.EmailEnviado = envioCliente.EmailEnviado;
                    resultado.WhatsAppEnviado = envioCliente.WhatsAppEnviado;
                    resultado.ErrosEnvio = envioCliente.Erros;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao cotar envio para {Nome}", envio.Nome);
                    resultado.Sucesso = false;
                    resultado.Erro = ex.Message;
                }

                resultados.Add(resultado);
            }

            return Ok(new CotarLoteResultado
            {
                Resultados = resultados,
                TotalSucesso = resultados.Count(r => r.Sucesso),
                TotalErro = resultados.Count(r => !r.Sucesso)
            });
        }

        /// <summary>
        /// Gera etiquetas em lote (já deve ter cotado antes para saber o serviçoId).
        /// </summary>
        [HttpPost("GerarEtiquetasLote")]
        public async Task<IActionResult> GerarEtiquetasLote([FromBody] GerarEtiquetasLoteInput input)
        {
            if (input.Envios == null || input.Envios.Count == 0)
                return BadRequest(new { message = "Nenhum envio para gerar etiqueta." });

            var resultados = new List<EtiquetaLoteItem>();

            foreach (var envio in input.Envios)
            {
                var resultado = new EtiquetaLoteItem
                {
                    Indice = envio.Indice,
                    Nome = envio.Nome
                };

                try
                {
                    var pesoKg = envio.PesoGramas / 1000m;
                    if (pesoKg < 0.3m) pesoKg = 0.3m;

                    var etiquetaRequest = new CriarEtiquetaRequest
                    {
                        Service = int.Parse(envio.ServicoId),
                        Platform = "Abrechozeira",
                        From = new EtiquetaEndereco(),
                        To = new EtiquetaDestinatario
                        {
                            Name = envio.Nome,
                            Address = envio.DestinatarioEndereco,
                            Number = envio.DestinatarioNumero,
                            Complement = envio.DestinatarioComplemento,
                            District = envio.DestinatarioBairro,
                            City = envio.DestinatarioCidade,
                            StateAbbr = envio.DestinatarioEstado,
                            PostalCode = envio.CepDestino.Replace("-", ""),
                            Email = envio.DestinatarioEmail,
                            Document = envio.DestinatarioCpf
                        },
                        Volumes = new CotacaoPacote
                        {
                            Weight = pesoKg,
                            Height = envio.Altura,
                            Width = envio.Largura,
                            Length = envio.Comprimento
                        },
                        Products = new List<EtiquetaProduto>
                        {
                            new EtiquetaProduto
                            {
                                Name = "Produtos Brechó",
                                Quantity = 1,
                                UnitaryValue = envio.Valor
                            }
                        },
                        Options = new EtiquetaOpcoes
                        {
                            InsuranceValue = envio.Valor,
                            NonCommercial = true
                        }
                    };

                    var etiqueta = await _superfrete.CriarEtiquetaAsync(etiquetaRequest);
                    resultado.Sucesso = true;
                    resultado.EtiquetaId = etiqueta.Id;
                    resultado.EtiquetaStatus = etiqueta.Status;
                    resultado.EtiquetaPreco = etiqueta.Price;

                    // Mapear transacaoId -> dados do envio para o Webhook
                    if (!string.IsNullOrWhiteSpace(envio.TransacaoId))
                    {
                        await SalvarMapeamento(envio.TransacaoId, etiqueta.Id, envio.DestinatarioEmail ?? "", envio.Nome, "Aguardando");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao gerar etiqueta para {Nome}", envio.Nome);
                    resultado.Sucesso = false;
                    resultado.Erro = ex.Message;
                }

                resultados.Add(resultado);
            }

            return Ok(new GerarEtiquetasLoteResultado
            {
                Resultados = resultados,
                TotalSucesso = resultados.Count(r => r.Sucesso),
                TotalErro = resultados.Count(r => !r.Sucesso),
                CustoTotal = resultados.Where(r => r.Sucesso).Sum(r => r.EtiquetaPreco ?? 0m)
            });
        }

        /// <summary>
        /// Reenvia o e-mail de rastreio manualmente para uma etiqueta já gerada.
        /// </summary>
        [HttpPost("EnviarRastreio/{etiquetaId}")]
        public async Task<IActionResult> EnviarRastreio(string etiquetaId, [FromQuery] string? email = null, [FromQuery] string? nome = null)
        {
            try
            {
                var info = await _superfrete.ObterEtiquetaAsync(etiquetaId);
                if (info == null)
                    return NotFound(new { message = "Etiqueta não encontrada." });

                if (string.IsNullOrWhiteSpace(info.Tracking))
                    return BadRequest(new { message = "Código de rastreio ainda não disponível para esta etiqueta." });

                var destinatarioEmail = email;
                var destinatarioNome = nome ?? info.Destinatario ?? "Cliente";

                // Se não passou e-mail por query, tentar buscar do destinatário da etiqueta
                if (string.IsNullOrWhiteSpace(destinatarioEmail))
                    return BadRequest(new { message = "Informe o e-mail do destinatário via query parameter 'email'." });

                var enviado = await _emailService.EnviarRastreioAsync(
                    destinatarioEmail,
                    destinatarioNome,
                    info.Tracking,
                    info.ServiceName);

                if (enviado)
                {
                    var map = await _context.EnvioLoteMap.FirstOrDefaultAsync(x => x.EtiquetaId == etiquetaId);
                    if (map != null)
                    {
                        map.EmailRastreioEnviado = true;
                        await _context.SaveChangesAsync();
                    }
                    return Ok(new { message = $"E-mail de rastreio enviado para {destinatarioEmail}." });
                }
                else
                    return StatusCode(500, new { message = "Falha ao enviar e-mail." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao reenviar rastreio para etiqueta {Id}", etiquetaId);
                return StatusCode(500, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Vincula a etiqueta à cotação. Se a cliente já pagou (pagamento chegou antes da etiqueta),
        /// libera a etiqueta no Superfrete na hora.
        /// </summary>
        private async Task SalvarMapeamento(string transacaoId, string etiquetaId, string email, string nome, string statusPagamento)
        {
            try
            {
                var map = await _context.EnvioLoteMap.FirstOrDefaultAsync(x => x.TransacaoId == transacaoId);
                if (map == null)
                {
                    map = new EnvioLoteMap
                    {
                        TransacaoId = transacaoId,
                        EtiquetaId = etiquetaId,
                        Email = email,
                        Nome = nome,
                        StatusPagamento = statusPagamento,
                        StatusSuperfrete = "Carrinho",
                        CreatedAt = DateTime.Now
                    };
                    _context.EnvioLoteMap.Add(map);
                }
                else
                {
                    map.EtiquetaId = etiquetaId;
                    map.Email = email;
                    map.Nome = nome;
                }
                await _context.SaveChangesAsync();

                if (map.StatusPagamento == "Pago")
                {
                    _logger.LogInformation("Cotação {TransacaoId} já estava paga; liberando a etiqueta {EtiquetaId}.", transacaoId, etiquetaId);
                    await _pagamentoFrete.LiberarEtiquetaAsync(map);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Não foi possível salvar o mapeamento TransacaoId: {TransacaoId}", transacaoId);
            }
        }

        /// <summary>
        /// Retorna todos os mapeamentos de pagamento salvos (para merge na listagem de envios).
        /// </summary>
        [HttpGet("Mapeamentos")]
        public async Task<IActionResult> GetMapeamentos()
        {
            try
            {
                var dict = await _context.EnvioLoteMap.ToDictionaryAsync(x => x.TransacaoId, x => new PaymentTagMapInfo
                {
                    EtiquetaId = x.EtiquetaId,
                    Email = x.Email,
                    Nome = x.Nome,
                    StatusPagamento = x.StatusPagamento,
                    StatusSuperfrete = x.StatusSuperfrete,
                    EmailCotacaoEnviado = x.EmailCotacaoEnviado,
                    EmailRastreioEnviado = x.EmailRastreioEnviado,
                    WhatsAppCotacaoEnviado = x.WhatsAppCotacaoEnviado,
                    WhatsAppRastreioEnviado = x.WhatsAppRastreioEnviado,
                    OrigemPagamento = x.OrigemPagamento,
                    PagoEm = x.PagoEm,
                    PagoManualPor = x.PagoManualPor
                });
                return Ok(dict);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao ler mapeamentos de pagamento");
                return StatusCode(500, new { message = "Erro ao ler mapeamentos." });
            }
        }

        /// <summary>
        /// Envia e-mails de rastreio em lote.
        /// </summary>
        [HttpPost("EnviarRastreioLote")]
        public async Task<IActionResult> EnviarRastreioLote([FromBody] EnviarRastreioLoteInput input)
        {
            if (input.Envios == null || input.Envios.Count == 0)
                return BadRequest(new { message = "Nenhum envio recebido para rastreio." });

            var resultado = new EnviarRastreioLoteResultado
            {
                Erros = new List<RastreioLoteItemErro>()
            };

            foreach (var envio in input.Envios)
            {
                if (string.IsNullOrWhiteSpace(envio.EtiquetaId) || string.IsNullOrWhiteSpace(envio.Email))
                {
                    resultado.TotalErro++;
                    resultado.Erros.Add(new RastreioLoteItemErro { Nome = envio.Nome ?? "Desconhecido", Erro = "EtiquetaId ou E-mail ausentes." });
                    continue;
                }

                try
                {
                    var info = await _superfrete.ObterEtiquetaAsync(envio.EtiquetaId);
                    if (info == null)
                    {
                        resultado.TotalErro++;
                        resultado.Erros.Add(new RastreioLoteItemErro { Nome = envio.Nome, Erro = "Etiqueta não encontrada." });
                        continue;
                    }

                    if (string.IsNullOrWhiteSpace(info.Tracking))
                    {
                        resultado.TotalErro++;
                        resultado.Erros.Add(new RastreioLoteItemErro { Nome = envio.Nome, Erro = "Código de rastreio ainda não gerado pela Superfrete/Correios." });
                        continue;
                    }

                    var destinatarioNome = envio.Nome ?? info.Destinatario ?? "Cliente";
                    var enviado = await _emailService.EnviarRastreioAsync(
                        envio.Email,
                        destinatarioNome,
                        info.Tracking,
                        info.ServiceName);

                    if (enviado)
                    {
                        resultado.TotalSucesso++;
                        var map = await _context.EnvioLoteMap.FirstOrDefaultAsync(x => x.EtiquetaId == envio.EtiquetaId);
                        if (map != null)
                        {
                            map.EmailRastreioEnviado = true;
                        }
                    }
                    else
                    {
                        resultado.TotalErro++;
                        resultado.Erros.Add(new RastreioLoteItemErro { Nome = destinatarioNome, Erro = "Falha ao enviar e-mail." });
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao reenviar rastreio para etiqueta {Id}", envio.EtiquetaId);
                    resultado.TotalErro++;
                    resultado.Erros.Add(new RastreioLoteItemErro { Nome = envio.Nome, Erro = "Erro interno ao processar rastreio." });
                }
            }

            await _context.SaveChangesAsync();
            return Ok(resultado);
        }

        // ─── Novos Endpoints de Reenvio e WhatsApp ────────────────────────────────────────

        /// <summary>
        /// Endpoint de TESTE: envia o template hello_world para um número informado.
        /// Usado para validar a integração com a Meta Cloud API sem depender de dados reais.
        /// Exemplo: POST /api/EnvioLote/TestarWhatsApp?telefone=5541999999999
        /// </summary>
        [HttpPost("TestarWhatsApp")]
        public async Task<IActionResult> TestarWhatsApp([FromQuery] string telefone, [FromQuery] string? template = null, [FromQuery] string? idioma = null)
        {
            if (string.IsNullOrWhiteSpace(telefone))
                return BadRequest(new { message = "Informe o parâmetro 'telefone' (ex: 5541999999999)." });

            var templateName = !string.IsNullOrWhiteSpace(template) 
                ? template 
                : (_config["WhatsApp:TemplateHelloWorld"] ?? "hello_world");

            var lang = !string.IsNullOrWhiteSpace(idioma) 
                ? idioma 
                : "en_US";

            var sucesso = await _whatsApp.SendTemplateMessageAsync(
                toPhoneNumber: telefone,
                templateName: templateName,
                bodyParameters: new List<string>(),
                buttonUrlParameter: "",
                languageCode: lang);

            if (sucesso)
                return Ok(new { message = $"✅ Template '{templateName}' ({lang}) enviado com sucesso para {telefone}." });

            return StatusCode(500, new { message = $"❌ Falha ao enviar template '{templateName}' ({lang}) para {telefone}. Verifique os logs da API." });
        }

        /// <summary>
        /// Endpoint de TESTE de Rastreio: envia o template rastreio_envio_v2 com dados de exemplo.
        /// Exemplo: POST /api/EnvioLote/TestarWhatsAppRastreio?telefone=5541999999999
        /// </summary>
        [HttpPost("TestarWhatsAppRastreio")]
        public async Task<IActionResult> TestarWhatsAppRastreio(
            [FromQuery] string telefone, 
            [FromQuery] string? nome = "Ana Souza", 
            [FromQuery] string? transportadora = "PAC Correios", 
            [FromQuery] string? codigoRastreio = "NL123456789BR")
        {
            if (string.IsNullOrWhiteSpace(telefone))
                return BadRequest(new { message = "Informe o parâmetro 'telefone' (ex: 5541999999999)." });

            var sucesso = await _whatsApp.EnviarRastreioEnvioAsync(
                destino: telefone,
                nomeCliente: nome ?? "Ana Souza",
                transportadora: transportadora ?? "PAC Correios",
                codigoRastreio: codigoRastreio ?? "NL123456789BR");

            if (sucesso)
                return Ok(new { message = $"✅ Template de rastreio enviado com sucesso para {telefone}." });

            return StatusCode(500, new { message = $"❌ Falha ao enviar template de rastreio para {telefone}. Verifique os logs da API." });
        }


        private bool WhatsAppConfigurado() =>
            !string.IsNullOrWhiteSpace(_config["WhatsApp:ApiToken"]) && !string.IsNullOrWhiteSpace(_config["WhatsApp:PhoneId"]);

        /// <summary>WhatsApp para mensagens com link de pagamento (cotação). Rastreio usa apenas WhatsAppConfigurado().</summary>
        private bool WhatsAppPagamentoDisponivel() => WhatsAppConfigurado() && _whatsApp.PagamentoHabilitado;

        /// <summary>
        /// Telefone do cliente para o WhatsApp: cadastro vinculado na importação, depois e-mail, depois nome.
        /// </summary>
        private async Task<string?> BuscarTelefoneClienteAsync(int? clienteId, string? email, string? nome)
        {
            string? telefone = null;
            if (clienteId.HasValue)
            {
                telefone = await _context.Pessoa.Where(p => p.Id == clienteId.Value)
                    .Select(p => p.Telefone).FirstOrDefaultAsync();
            }
            if (string.IsNullOrWhiteSpace(telefone) && !string.IsNullOrWhiteSpace(email))
            {
                telefone = await _context.Pessoa.Where(p => p.Email == email)
                    .Select(p => p.Telefone).FirstOrDefaultAsync();
            }
            if (string.IsNullOrWhiteSpace(telefone) && !string.IsNullOrWhiteSpace(nome))
            {
                telefone = await _context.Pessoa.Where(p => p.Nome == nome)
                    .Select(p => p.Telefone).FirstOrDefaultAsync();
            }
            return string.IsNullOrWhiteSpace(telefone) ? null : telefone;
        }

        private class EnvioCotacaoResultado
        {
            public bool EmailEnviado { get; set; }
            public bool WhatsAppEnviado { get; set; }
            public bool SemEmail { get; set; }
            public bool SemWhatsApp { get; set; }
            public List<string> Erros { get; set; } = new();
        }

        /// <summary>
        /// Envia a cotação ao cliente pelos canais pedidos (e-mail e/ou WhatsApp).
        /// Gera o link de checkout da InfinitePay se ainda não existir. Usado na cotação (automático)
        /// e no reenvio pela lista de envios.
        /// </summary>
        private async Task<EnvioCotacaoResultado> EnviarCotacaoAoClienteAsync(EnvioLoteMap map, int? clienteId, bool viaEmail, bool viaWhatsApp)
        {
            var res = new EnvioCotacaoResultado();
            var preco = map.PrecoRecomendado.GetValueOrDefault();
            var servico = map.ServicoRecomendado ?? "PAC";
            var telefone = await BuscarTelefoneClienteAsync(clienteId, map.Email, map.Nome);

            res.SemEmail = string.IsNullOrWhiteSpace(map.Email);
            res.SemWhatsApp = !WhatsAppPagamentoDisponivel() || string.IsNullOrWhiteSpace(telefone);
            var enviarEmail = viaEmail && !res.SemEmail;
            var enviarWhatsApp = viaWhatsApp && !res.SemWhatsApp;

            if (viaEmail && res.SemEmail) res.Erros.Add("Cliente sem e-mail cadastrado");
            if (viaWhatsApp && !WhatsAppPagamentoDisponivel()) res.Erros.Add("WhatsApp de cotação desativado");
            else if (viaWhatsApp && res.SemWhatsApp) res.Erros.Add("Cliente sem telefone cadastrado");
            if (!enviarEmail && !enviarWhatsApp) return res;

            try
            {
                if (string.IsNullOrWhiteSpace(map.LinkCheckout))
                {
                    map.LinkCheckout = await _infinitePay.GerarLinkCheckoutAsync(preco, $"Frete {servico}", map.TransacaoId, map.Nome, map.Email ?? "");
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao gerar link de checkout da cotação {TransacaoId}", map.TransacaoId);
                res.Erros.Add($"Falha ao gerar link de pagamento: {ex.Message}");
                return res;
            }

            if (enviarEmail)
            {
                try
                {
                    res.EmailEnviado = await _emailService.EnviarCotacaoAsync(
                        map.Email, map.Nome, servico, preco,
                        map.PrecoPAC ?? 0m, map.PrecoSEDEX ?? 0m, map.LinkCheckout);
                    if (res.EmailEnviado) map.EmailCotacaoEnviado = true;
                    else res.Erros.Add("Falha ao enviar e-mail");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao enviar e-mail de cotação para {Email}", map.Email);
                    res.Erros.Add("Falha ao enviar e-mail");
                }
            }

            if (enviarWhatsApp)
            {
                var slugCheckout = ExtrairSlugCheckout(map.LinkCheckout);
                if (slugCheckout == null)
                {
                    _logger.LogError("Link de checkout em formato inesperado para o WhatsApp: {Link}", map.LinkCheckout);
                    res.Erros.Add("Link de pagamento em formato inesperado para o WhatsApp");
                }
                else
                {
                    try
                    {
                        res.WhatsAppEnviado = await _whatsApp.EnviarCotacaoFreteAsync(
                            destino: telefone!,
                            nomeCliente: map.Nome,
                            valorFrete: preco,
                            modalidade: servico,
                            checkoutSlug: slugCheckout);
                        if (res.WhatsAppEnviado) map.WhatsAppCotacaoEnviado = true;
                        else res.Erros.Add("Falha ao enviar WhatsApp");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Erro ao enviar WhatsApp de cotação para {Nome}", map.Nome);
                        res.Erros.Add("Falha ao enviar WhatsApp");
                    }
                }
            }

            await _context.SaveChangesAsync();
            return res;
        }

        /// <summary>
        /// Envia (ou reenvia) a cotação ao cliente pelos canais escolhidos.
        /// </summary>
        [HttpPost("EnviarCotacaoCliente/{transacaoId}")]
        public async Task<IActionResult> EnviarCotacaoCliente(string transacaoId, [FromQuery] int? clienteId = null,
            [FromQuery] bool email = true, [FromQuery] bool whatsapp = true)
        {
            if (!email && !whatsapp)
                return BadRequest(new { message = "Escolha ao menos um canal de envio." });

            var map = await _context.EnvioLoteMap.FirstOrDefaultAsync(x => x.TransacaoId == transacaoId);
            if (map == null) return NotFound(new { message = "Cotação não encontrada." });

            var res = await EnviarCotacaoAoClienteAsync(map, clienteId, email, whatsapp);
            return Ok(new
            {
                emailEnviado = res.EmailEnviado,
                whatsAppEnviado = res.WhatsAppEnviado,
                semEmail = res.SemEmail,
                semWhatsApp = res.SemWhatsApp,
                erros = res.Erros
            });
        }

        /// <summary>
        /// Marca a cotação como paga manualmente (ex.: o aviso da InfinitePay não chegou) e libera a etiqueta.
        /// Fica registrado no banco como origem "Manual", com data e usuário.
        /// </summary>
        [Authorize]
        [HttpPost("MarcarComoPago/{transacaoId}")]
        public async Task<IActionResult> MarcarComoPago(string transacaoId)
        {
            var map = await _context.EnvioLoteMap.FirstOrDefaultAsync(x => x.TransacaoId == transacaoId);
            if (map == null) return NotFound(new { message = "Cotação não encontrada." });
            if (map.StatusPagamento == "Pago") return BadRequest(new { message = "Esta cotação já está paga." });

            var usuario = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst(ClaimTypes.Email)?.Value ?? "desconhecido";
            _logger.LogWarning("Cotação {TransacaoId} marcada como paga manualmente por {Usuario}.", transacaoId, usuario);

            await _pagamentoFrete.ConfirmarPagamentoAsync(map, PagamentoFreteService.OrigemManual, usuario);

            return Ok(new
            {
                statusPagamento = map.StatusPagamento,
                statusSuperfrete = map.StatusSuperfrete,
                origemPagamento = map.OrigemPagamento,
                pagoEm = map.PagoEm,
                pagoManualPor = map.PagoManualPor,
                message = string.IsNullOrEmpty(map.EtiquetaId)
                    ? "Pagamento registrado. A etiqueta será liberada quando for gerada."
                    : map.StatusSuperfrete == "Liberada"
                        ? "Pagamento registrado e etiqueta liberada no Superfrete."
                        : "Pagamento registrado, mas houve erro ao liberar a etiqueta no Superfrete."
            });
        }

        [HttpPost("ReenviarCotacao/{transacaoId}")]
        public async Task<IActionResult> ReenviarCotacao(string transacaoId)
        {
            try
            {
                var map = await _context.EnvioLoteMap.FirstOrDefaultAsync(x => x.TransacaoId == transacaoId);
                if (map == null) return NotFound(new { message = "Cotação não encontrada." });

                if (string.IsNullOrWhiteSpace(map.LinkCheckout) || string.IsNullOrWhiteSpace(map.Email))
                    return BadRequest(new { message = "Dados incompletos (link ou e-mail faltando)." });

                var enviado = await _emailService.EnviarCotacaoAsync(
                    map.Email, map.Nome, map.ServicoRecomendado ?? "PAC",
                    map.PrecoRecomendado ?? 0m, map.PrecoPAC ?? 0m, map.PrecoSEDEX ?? 0m,
                    map.LinkCheckout);

                if (enviado)
                {
                    map.EmailCotacaoEnviado = true;
                    await _context.SaveChangesAsync();
                    return Ok(new { message = "E-mail de cotação reenviado." });
                }
                return StatusCode(500, new { message = "Falha ao reenviar e-mail." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [HttpPost("EnviarWhatsAppCotacao/{transacaoId}")]
        public async Task<IActionResult> EnviarWhatsAppCotacao(string transacaoId)
        {
            if (!_whatsApp.PagamentoHabilitado)
                return BadRequest(new { message = "Envio de cotação por WhatsApp temporariamente desativado. Use o envio por e-mail." });

            var map = await _context.EnvioLoteMap.FirstOrDefaultAsync(x => x.TransacaoId == transacaoId);
            if (map == null) return NotFound(new { message = "Cotação não encontrada." });

            // Buscar o telefone do cliente na tabela Pessoa
            string? telefone = null;
            if (!string.IsNullOrWhiteSpace(map.Email))
            {
                var pessoa = await _context.Pessoa.FirstOrDefaultAsync(p => p.Email == map.Email);
                telefone = pessoa?.Telefone;
            }
            if (string.IsNullOrWhiteSpace(telefone))
            {
                var pessoa = await _context.Pessoa.FirstOrDefaultAsync(p => p.Nome == map.Nome);
                telefone = pessoa?.Telefone;
            }

            if (string.IsNullOrWhiteSpace(telefone))
            {
                return BadRequest(new { message = "Telefone do cliente não encontrado no cadastro do sistema." });
            }

            var preco = map.PrecoRecomendado.GetValueOrDefault();
            var servico = map.ServicoRecomendado ?? "Correios";
            var mensagem = $"Olá {map.Nome}, o frete da sua compra ficou em R$ {preco:F2} via {servico}. Segue o link para pagamento: {map.LinkCheckout}";
            
            // Abordagem via Cloud API da Meta se configurado
            var token = _config["WhatsApp:ApiToken"];
            var phoneId = _config["WhatsApp:PhoneId"];
            
            if (!string.IsNullOrWhiteSpace(token) && !string.IsNullOrWhiteSpace(phoneId))
            {
                var slugCheckout = ExtrairSlugCheckout(map.LinkCheckout);
                if (slugCheckout == null)
                    return BadRequest(new { message = "Cotação sem link de pagamento válido. Envie a cotação pela tela de importação." });

                var sucesso = await _whatsApp.EnviarCotacaoFreteAsync(
                    destino: telefone,
                    nomeCliente: map.Nome,
                    valorFrete: preco,
                    modalidade: servico,
                    checkoutSlug: slugCheckout);
                
                if (sucesso)
                {
                    map.WhatsAppCotacaoEnviado = true;
                    await _context.SaveChangesAsync();
                    return Ok(new { message = "Enviado via API da Meta", url = "", hibrido = false });
                }
                return StatusCode(500, new { message = "Falha ao enviar WhatsApp via API da Meta." });
            }

            // Fallback: Link manual
            var linkManual = $"https://wa.me/{WhatsAppService.NormalizarNumeroBrasil(telefone)}?text={Uri.EscapeDataString(mensagem)}";
            map.WhatsAppCotacaoEnviado = true;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Link manual gerado", url = linkManual, hibrido = true });
        }

        /// <summary>
        /// O botão dos templates de pagamento tem a URL fixa "https://checkout.infinitepay.io/{{1}}",
        /// então o parâmetro é o restante do link gerado pela InfinitePay (ex: "abrechozeira?lenc=...").
        /// </summary>
        private static string? ExtrairSlugCheckout(string? linkCheckout)
        {
            const string prefixo = "https://checkout.infinitepay.io/";
            if (string.IsNullOrWhiteSpace(linkCheckout) || !linkCheckout.StartsWith(prefixo, StringComparison.OrdinalIgnoreCase))
                return null;

            var slug = linkCheckout.Substring(prefixo.Length);
            return string.IsNullOrWhiteSpace(slug) ? null : slug;
        }

        [HttpPost("EnviarWhatsAppRastreio/{etiquetaId}")]
        public async Task<IActionResult> EnviarWhatsAppRastreio(string etiquetaId, [FromQuery] string? nome = null, [FromQuery] string? email = null)
        {
            var map = await _context.EnvioLoteMap.FirstOrDefaultAsync(x => x.EtiquetaId == etiquetaId);
            var info = await _superfrete.ObterEtiquetaAsync(etiquetaId);

            string nomeCliente = map?.Nome ?? (!string.IsNullOrWhiteSpace(nome) ? nome : info?.Destinatario ?? "Cliente");
            string? emailCliente = map?.Email ?? (!string.IsNullOrWhiteSpace(email) ? email : info?.Email);
            if (info != null && (info.Status == "delivered" || info.Status == "cancelled"))
            {
                var statusMsg = info.Status == "delivered" ? "já foi entregue ao destinatário" : "foi cancelada";
                return BadRequest(new { message = $"Esta encomenda {statusMsg}." });
            }

            string? telefone = null;

            // 1. Tenta buscar na tabela Pessoa pelo E-mail
            if (!string.IsNullOrWhiteSpace(emailCliente))
            {
                var pessoa = await _context.Pessoa.FirstOrDefaultAsync(p => p.Email == emailCliente);
                telefone = pessoa?.Telefone;
            }

            // 2. Tenta buscar na tabela Pessoa pelo Nome Completo ou pelo Primeiro Nome
            if (string.IsNullOrWhiteSpace(telefone) && !string.IsNullOrWhiteSpace(nomeCliente))
            {
                var primeiroNome = nomeCliente.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0].Trim();
                var pessoa = await _context.Pessoa.FirstOrDefaultAsync(p => 
                    p.Nome != null && (
                        p.Nome == nomeCliente || 
                        p.Nome == primeiroNome || 
                        p.Nome.StartsWith(primeiroNome) ||
                        nomeCliente.StartsWith(p.Nome)
                    )
                );
                telefone = pessoa?.Telefone;
            }

            // 3. Fallback: telefone presente na própria etiqueta da Superfrete
            if (string.IsNullOrWhiteSpace(telefone) && info != null)
            {
                telefone = info.Phone;
            }

            if (string.IsNullOrWhiteSpace(telefone))
            {
                return BadRequest(new { message = "Telefone do cliente não encontrado no cadastro do sistema nem na etiqueta." });
            }

            var transportadora = !string.IsNullOrWhiteSpace(info.ServiceName) ? info.ServiceName : "PAC Correios";
            var mensagem = $"Olá {nomeCliente}, sua encomenda foi postada via {transportadora}! O código de rastreio é {info.Tracking}. Você pode acompanhar pelo site dos Correios.";
            
            // Abordagem via Cloud API da Meta se configurado
            var token = _config["WhatsApp:ApiToken"];
            var phoneId = _config["WhatsApp:PhoneId"];
            
            if (!string.IsNullOrWhiteSpace(token) && !string.IsNullOrWhiteSpace(phoneId))
            {
                var sucesso = await _whatsApp.EnviarRastreioEnvioAsync(
                    destino: telefone,
                    nomeCliente: nomeCliente,
                    transportadora: transportadora,
                    codigoRastreio: info.Tracking);
                
                if (sucesso)
                {
                    if (map != null)
                    {
                        map.WhatsAppRastreioEnviado = true;
                        await _context.SaveChangesAsync();
                    }
                    return Ok(new { message = "Enviado via API da Meta", url = "", hibrido = false });
                }
                return StatusCode(500, new { message = "Falha ao enviar WhatsApp via API da Meta." });
            }

            var linkManual = $"https://wa.me/{WhatsAppService.NormalizarNumeroBrasil(telefone)}?text={Uri.EscapeDataString(mensagem)}";
            if (map != null)
            {
                map.WhatsAppRastreioEnviado = true;
                await _context.SaveChangesAsync();
            }
            return Ok(new { message = "Link manual gerado", url = linkManual, hibrido = true });
        }

        /// <summary>
        /// Verifica o status atualizado do pagamento e da superfrete para uma lista de transações.
        /// </summary>
        [HttpPost("VerificarStatus")]
        public async Task<IActionResult> VerificarStatus([FromBody] List<string> transacaoIds)
        {
            var resultados = new List<object>();

            try
            {
                var maps = await _context.EnvioLoteMap.Where(x => transacaoIds.Contains(x.TransacaoId)).ToListAsync();
                var dict = maps.ToDictionary(x => x.TransacaoId);

                foreach (var transacaoId in transacaoIds)
                {
                    if (dict.TryGetValue(transacaoId, out var info))
                    {
                        // Sincroniza em tempo real com a Superfrete (caso o rastreio ou status mudou lá)
                        if (!string.IsNullOrEmpty(info.EtiquetaId))
                        {
                            try
                            {
                                var etiquetaInfo = await _superfrete.ObterEtiquetaAsync(info.EtiquetaId);
                                if (etiquetaInfo != null && !string.IsNullOrEmpty(etiquetaInfo.Status))
                                {
                                    info.StatusSuperfrete = etiquetaInfo.Status;
                                }

                                // Se está pago na loja mas falhou/ainda está no carrinho na Superfrete, tenta o checkout de novo!
                                if (info.StatusPagamento == "Pago" && info.StatusSuperfrete != "Liberada" && info.StatusSuperfrete != "Cancelada")
                                {
                                    var sucessoCheckout = await _superfrete.CheckoutAsync(info.EtiquetaId);
                                    if (sucessoCheckout)
                                    {
                                        info.StatusSuperfrete = "Liberada";
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning(ex, "Falha ao sincronizar status da Superfrete para a etiqueta {Id}", info.EtiquetaId);
                            }
                        }

                        resultados.Add(new
                        {
                            TransacaoId = transacaoId,
                            StatusPagamento = info.StatusPagamento,
                            StatusSuperfrete = info.StatusSuperfrete
                        });
                    }
                    else
                    {
                        resultados.Add(new
                        {
                            TransacaoId = transacaoId,
                            StatusPagamento = "Aguardando",
                            StatusSuperfrete = "Não Criada"
                        });
                    }
                }

                await _context.SaveChangesAsync();

                return Ok(resultados);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar status em lote.");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        // ─── Parsing de texto do WhatsApp ────────────────────────────────────────

        private List<EnvioParseado> ParseWhatsAppText(string texto)
        {
            var envios = new List<EnvioParseado>();

            // Normalizar quebras de linha
            texto = texto.Replace("\r\n", "\n").Replace("\r", "\n");

            // Remover linhas com "Encaminhada" e "Relatório"
            var linhas = texto.Split('\n')
                .Select(l => l.Trim())
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .Where(l => !l.Contains("Encaminhada", StringComparison.OrdinalIgnoreCase))
                .Where(l => !l.StartsWith("Relatório", StringComparison.OrdinalIgnoreCase))
                .Where(l => !Regex.IsMatch(l, @"^\d{1,2}:\d{2}$")) // Remove timestamps tipo "17:18"
                .ToList();

            // Padrões regex
            var regexValor = new Regex(@"R\$\s*(\d[\d.,]*|\?)", RegexOptions.IgnoreCase);
            var regexPeso = new Regex(@"^(\d+)\s*g$", RegexOptions.IgnoreCase);
            var regexDimensoes = new Regex(@"^(\d+)\s*x\s*(\d+)\s*x\s*(\d+)$", RegexOptions.IgnoreCase);

            // Parsear por blocos: detectar padrão Nome → Valor → Peso → Dimensões
            int indice = 0;
            int i = 0;

            while (i < linhas.Count)
            {
                // Tentar detectar um bloco de envio
                string? nome = null;
                decimal? valor = null;
                int peso = 0;
                decimal altura = 0, largura = 0, comprimento = 0;
                bool temDados = false;

                // Linha 1: Nome (não é valor, nem peso, nem dimensão)
                if (i < linhas.Count
                    && !regexValor.IsMatch(linhas[i])
                    && !regexPeso.IsMatch(linhas[i])
                    && !regexDimensoes.IsMatch(linhas[i]))
                {
                    nome = linhas[i];
                    i++;
                    temDados = true;
                }

                // Linha 2: Valor (R$ xxx,xx ou R$ ?)
                if (i < linhas.Count && regexValor.IsMatch(linhas[i]))
                {
                    var match = regexValor.Match(linhas[i]);
                    var valorStr = match.Groups[1].Value;
                    if (valorStr != "?")
                    {
                        // Tratar formato brasileiro: 760,00 ou 1.760,00
                        valorStr = valorStr.Replace(".", "").Replace(",", ".");
                        if (decimal.TryParse(valorStr, System.Globalization.NumberStyles.Any,
                            System.Globalization.CultureInfo.InvariantCulture, out var v))
                        {
                            valor = v;
                        }
                    }
                    i++;
                    temDados = true;
                }

                // Linha 3: Peso (ex: 135g)
                if (i < linhas.Count && regexPeso.IsMatch(linhas[i]))
                {
                    var match = regexPeso.Match(linhas[i]);
                    int.TryParse(match.Groups[1].Value, out peso);
                    i++;
                    temDados = true;
                }

                // Linha 4: Dimensões (ex: 3x13x20) → Altura x Largura x Comprimento
                if (i < linhas.Count && regexDimensoes.IsMatch(linhas[i]))
                {
                    var match = regexDimensoes.Match(linhas[i]);
                    decimal.TryParse(match.Groups[1].Value, out altura);
                    decimal.TryParse(match.Groups[2].Value, out largura);
                    decimal.TryParse(match.Groups[3].Value, out comprimento);
                    i++;
                    temDados = true;
                }

                if (temDados && !string.IsNullOrWhiteSpace(nome))
                {
                    indice++;
                    envios.Add(new EnvioParseado
                    {
                        Indice = indice,
                        Nome = nome,
                        Valor = valor,
                        PesoGramas = peso,
                        Altura = altura,
                        Largura = largura,
                        Comprimento = comprimento
                    });
                }
                else if (!temDados)
                {
                    // Linha não reconhecida, avança
                    i++;
                }
            }

            return envios;
        }

        // ─── Busca de cliente por nome ───────────────────────────────────────────

        private async Task BuscarClientePorNome(EnvioParseado envio)
        {
            try
            {
                // Busca por nome exato primeiro
                var pessoa = await _context.Pessoa
                    .FirstOrDefaultAsync(p => p.Nome != null && p.Nome.ToLower() == envio.Nome.ToLower());

                // Se não encontrar, busca parcial
                if (pessoa == null)
                {
                    var nomePartes = envio.Nome.Split(' ');
                    if (nomePartes.Length >= 2)
                    {
                        var primeiroNome = nomePartes[0].ToLower();
                        var ultimoNome = nomePartes[^1].ToLower();

                        pessoa = await _context.Pessoa
                            .FirstOrDefaultAsync(p =>
                                p.Nome != null &&
                                p.Nome.ToLower().Contains(primeiroNome) &&
                                p.Nome.ToLower().Contains(ultimoNome));
                    }
                }

                if (pessoa != null)
                {
                    envio.ClienteEncontrado = true;
                    envio.ClienteId = pessoa.Id;
                    envio.ClienteNomeSistema = pessoa.Nome;

                    envio.DestinatarioCpf = pessoa.CPF;
                    envio.DestinatarioEmail = pessoa.Email;

                    // Buscar endereço do cliente
                    var endereco = await _context.Endereco
                        .FirstOrDefaultAsync(e => e.PessoaID == pessoa.Id);

                    if (endereco != null)
                    {
                        envio.EnderecoId = endereco.Id;
                        envio.CepDestino = endereco.CEP ?? "";
                        envio.EnderecoCompleto = $"{endereco.Logradouro}, {endereco.Unidade} - {endereco.Bairro}, {endereco.Localidade}/{endereco.Estado}";
                        
                        envio.DestinatarioEndereco = endereco.Logradouro;
                        envio.DestinatarioNumero = endereco.Unidade ?? "S/N";
                        envio.DestinatarioComplemento = endereco.Complemento;
                        envio.DestinatarioBairro = endereco.Bairro;
                        envio.DestinatarioCidade = endereco.Localidade;
                        envio.DestinatarioEstado = endereco.Estado;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Erro ao buscar cliente por nome: {Nome}", envio.Nome);
            }
        }
    }
}
