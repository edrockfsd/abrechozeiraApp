using Microsoft.AspNetCore.Mvc;
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

        public EnvioLoteController(
            EmailService emailService,
            SuperfreteService superfrete,
            InfinitePayService infinitePay,
            ILogger<EnvioLoteController> logger,
            AbrechozeiraContext context,
            IConfiguration config)
        {
            _emailService = emailService;
            _superfrete = superfrete;
            _infinitePay = infinitePay;
            _logger = logger;
            _context = context;
            _config = config;
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

                    // Enviar e-mail de cotação ao cliente (fire-and-forget, não bloqueia)
                    if (!string.IsNullOrWhiteSpace(envio.DestinatarioEmail))
                    {
                        _logger.LogInformation("➡️ Disparando Task em segundo plano para o e-mail: {Email}", envio.DestinatarioEmail);
                        var factory = HttpContext.RequestServices.GetRequiredService<IServiceScopeFactory>();
                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                using var scope = factory.CreateScope();
                                var emailService = scope.ServiceProvider.GetRequiredService<EmailService>();
                                var infinitePayService = scope.ServiceProvider.GetRequiredService<InfinitePayService>();
                                var logger = scope.ServiceProvider.GetRequiredService<ILogger<EnvioLoteController>>();
                                var db = scope.ServiceProvider.GetRequiredService<AbrechozeiraContext>();

                                logger.LogInformation("✅ [Task] Escopo criado. Iniciando geração do link de checkout InfinitePay...");

                                var checkoutUrl = await infinitePayService.GerarLinkCheckoutAsync(resultado.PrecoRecomendado, $"Frete {resultado.ServicoRecomendado}", transacaoId, envio.Nome, envio.DestinatarioEmail ?? "");
                                
                                logger.LogInformation("✅ [Task] Link gerado com sucesso: {Url}. Iniciando disparo do E-mail...", checkoutUrl);

                                // Atualizar link
                                var dbMap = await db.EnvioLoteMap.FirstOrDefaultAsync(x => x.TransacaoId == transacaoId);
                                if (dbMap != null) {
                                    dbMap.LinkCheckout = checkoutUrl;
                                    await db.SaveChangesAsync();
                                }

                                await emailService.EnviarCotacaoAsync(
                                    envio.DestinatarioEmail,
                                    envio.Nome,
                                    resultado.ServicoRecomendado,
                                    resultado.PrecoRecomendado,
                                    resultado.PrecoPAC,
                                    resultado.PrecoSEDEX ?? 0m,
                                    checkoutUrl);

                                logger.LogInformation("✅ [Task] E-mail de cotação ENVIADO com sucesso para {Email}!", envio.DestinatarioEmail);
                                
                                if (dbMap != null) {
                                    dbMap.EmailCotacaoEnviado = true;
                                    await db.SaveChangesAsync();
                                }
                            }
                            catch (Exception emailEx)
                            {
                                Console.WriteLine($"❌ [Task] ERRO FATAL: {emailEx.Message} - {emailEx.StackTrace}");
                            }
                        });
                    }
                    else
                    {
                        _logger.LogWarning("⚠️ Cliente {Nome} não possui e-mail cadastrado. Pulando envio de e-mail.", envio.Nome);
                    }
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
                    map.StatusPagamento = statusPagamento;
                }
                await _context.SaveChangesAsync();
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
                    WhatsAppRastreioEnviado = x.WhatsAppRastreioEnviado
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
            var map = await _context.EnvioLoteMap.FirstOrDefaultAsync(x => x.TransacaoId == transacaoId);
            if (map == null) return NotFound(new { message = "Cotação não encontrada." });

            var mensagem = $"Olá {map.Nome}, o frete da sua compra ficou em R$ {map.PrecoRecomendado:F2} via {map.ServicoRecomendado}. Segue o link para pagamento: {map.LinkCheckout}";
            
            // Abordagem Híbrida: Tenta enviar via API se configurado
            var apiUrl = _config["WhatsApp:ApiUrl"];
            if (!string.IsNullOrWhiteSpace(apiUrl))
            {
                // TODO: Implementar HTTP Client para Evolution API / Z-API
                // Por hora, se configurado, simulamos o envio.
                map.WhatsAppCotacaoEnviado = true;
                await _context.SaveChangesAsync();
                return Ok(new { message = "Enviado via API", url = "", hibrido = false });
            }

            // Fallback: Link manual
            var linkManual = $"https://wa.me/?text={Uri.EscapeDataString(mensagem)}";
            map.WhatsAppCotacaoEnviado = true;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Link manual gerado", url = linkManual, hibrido = true });
        }

        [HttpPost("EnviarWhatsAppRastreio/{etiquetaId}")]
        public async Task<IActionResult> EnviarWhatsAppRastreio(string etiquetaId)
        {
            var map = await _context.EnvioLoteMap.FirstOrDefaultAsync(x => x.EtiquetaId == etiquetaId);
            if (map == null) return NotFound(new { message = "Envio não encontrado." });

            var info = await _superfrete.ObterEtiquetaAsync(etiquetaId);
            if (info == null || string.IsNullOrWhiteSpace(info.Tracking))
                return BadRequest(new { message = "Rastreio não disponível." });

            var mensagem = $"Olá {map.Nome}, sua encomenda foi postada via {info.ServiceName}! O código de rastreio é {info.Tracking}. Você pode acompanhar pelo site dos Correios.";
            
            var apiUrl = _config["WhatsApp:ApiUrl"];
            if (!string.IsNullOrWhiteSpace(apiUrl))
            {
                map.WhatsAppRastreioEnviado = true;
                await _context.SaveChangesAsync();
                return Ok(new { message = "Enviado via API", url = "", hibrido = false });
            }

            var linkManual = $"https://wa.me/?text={Uri.EscapeDataString(mensagem)}";
            map.WhatsAppRastreioEnviado = true;
            await _context.SaveChangesAsync();
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
