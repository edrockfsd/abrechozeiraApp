using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ABrechozeiraApp.Models;
using ABrechozeiraApp.Services;
using System.Text.RegularExpressions;

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
        public int ServicoIdRecomendado { get; set; }
        public string ServicoRecomendado { get; set; } = "";
        public decimal PrecoRecomendado { get; set; }
        public string MotivoEscolha { get; set; } = "";
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
        public int ServicoId { get; set; }
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

    public class GerarEtiquetasLoteResultado
    {
        public List<EtiquetaLoteItem> Resultados { get; set; } = new();
        public int TotalSucesso { get; set; }
        public int TotalErro { get; set; }
        public decimal CustoTotal { get; set; }
    }

    // ─── Controller ──────────────────────────────────────────────────────────────

    [ApiController]
    [Route("api/[controller]")]
    public class EnvioLoteController : ControllerBase
    {
        private readonly AbrechozeiraContext _context;
        private readonly SuperfreteService _superfrete;
        private readonly IConfiguration _config;
        private readonly ILogger<EnvioLoteController> _logger;

        public EnvioLoteController(
            AbrechozeiraContext context,
            SuperfreteService superfrete,
            IConfiguration config,
            ILogger<EnvioLoteController> logger)
        {
            _context = context;
            _superfrete = superfrete;
            _config = config;
            _logger = logger;
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
                var resultado = new CotacaoLoteItem
                {
                    Indice = envio.Indice,
                    Nome = envio.Nome
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

                    // Aplicar regra PAC/SEDEX
                    if (pac == null && sedex != null)
                    {
                        resultado.ServicoIdRecomendado = 2;
                        resultado.ServicoRecomendado = "SEDEX";
                        resultado.PrecoRecomendado = sedex.Price;
                        resultado.MotivoEscolha = "PAC indisponível";
                    }
                    else if (sedex != null && sedex.Price <= pac!.Price)
                    {
                        resultado.ServicoIdRecomendado = 2;
                        resultado.ServicoRecomendado = "SEDEX";
                        resultado.PrecoRecomendado = sedex.Price;
                        resultado.MotivoEscolha = $"SEDEX mais barato (R${sedex.Price:F2} vs PAC R${pac.Price:F2})";
                    }
                    else if (sedex != null && (sedex.Price - pac!.Price) <= 3.0m)
                    {
                        resultado.ServicoIdRecomendado = 2;
                        resultado.ServicoRecomendado = "SEDEX";
                        resultado.PrecoRecomendado = sedex.Price;
                        resultado.MotivoEscolha = $"SEDEX +R${(sedex.Price - pac.Price):F2} do PAC";
                    }
                    else
                    {
                        resultado.ServicoIdRecomendado = 1;
                        resultado.ServicoRecomendado = "PAC";
                        resultado.PrecoRecomendado = pac!.Price;
                        resultado.MotivoEscolha = sedex != null
                            ? $"SEDEX R${(sedex.Price - pac.Price):F2} acima"
                            : "SEDEX indisponível";
                    }

                    resultado.Sucesso = true;
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
                        Service = envio.ServicoId,
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
                CustoTotal = resultados.Where(r => r.Sucesso && r.EtiquetaPreco.HasValue).Sum(r => r.EtiquetaPreco!.Value)
            });
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
