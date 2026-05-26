using Microsoft.AspNetCore.Mvc;
using ABrechozeiraApp.Services;

namespace ABrechozeiraApp.Controllers
{
    // ─── Input DTOs (frontend → API) ─────────────────────────────────────────────
    public class CotarFreteInput
    {
        public string CepOrigem { get; set; } = "";
        public string CepDestino { get; set; } = "";
        public string Servicos { get; set; } = "1,2,17,3";
        public decimal Peso { get; set; }
        public decimal Altura { get; set; }
        public decimal Largura { get; set; }
        public decimal Comprimento { get; set; }
        public decimal ValorSeguro { get; set; } = 0;
    }

    public class CriarEtiquetaInput
    {
        public int ServicoId { get; set; }
        // Destinatário
        public string DestinatarioNome { get; set; } = "";
        public string DestinatarioEndereco { get; set; } = "";
        public string DestinatarioNumero { get; set; } = "";
        public string DestinatarioBairro { get; set; } = "";
        public string DestinatarioCidade { get; set; } = "";
        public string DestinatarioEstado { get; set; } = "";
        public string DestinatarioCep { get; set; } = "";
        public string? DestinatarioEmail { get; set; }
        public string? DestinatarioCpf { get; set; }
        // Dimensões (da cotação)
        public decimal Peso { get; set; }
        public decimal Altura { get; set; }
        public decimal Largura { get; set; }
        public decimal Comprimento { get; set; }
        // Produtos (declaração de conteúdo)
        public List<ProdutoEtiquetaInput> Produtos { get; set; } = new();
        // Remetente (opcional; se não enviado usa padrão do appsettings)
        public string? RemetenteNome { get; set; }
        public string? RemetenteEndereco { get; set; }
        public string? RemetenteNumero { get; set; }
        public string? RemetenteBairro { get; set; }
        public string? RemetenteCidade { get; set; }
        public string? RemetenteEstado { get; set; }
        public string? RemetenteCep { get; set; }
    }

    public class ProdutoEtiquetaInput
    {
        public string Nome { get; set; } = "";
        public int Quantidade { get; set; } = 1;
        public decimal ValorUnitario { get; set; }
    }

    // ─── Controller ───────────────────────────────────────────────────────────────
    [ApiController]
    [Route("api/[controller]")]
    public class SuperfreteController : ControllerBase
    {
        private readonly SuperfreteService _superfrete;
        private readonly ILogger<SuperfreteController> _logger;

        public SuperfreteController(SuperfreteService superfrete, ILogger<SuperfreteController> logger)
        {
            _superfrete = superfrete;
            _logger = logger;
        }

        /// <summary>Calcula cotações de frete com base no CEP e dimensões do pacote.</summary>
        [HttpPost("CotarFrete")]
        public async Task<IActionResult> CotarFrete([FromBody] CotarFreteInput input)
        {
            try
            {
                var request = new CotacaoFreteRequest
                {
                    From = new CotacaoEndereco { PostalCode = input.CepOrigem.Replace("-", "") },
                    To = new CotacaoEndereco { PostalCode = input.CepDestino.Replace("-", "") },
                    Services = input.Servicos,
                    Options = new CotacaoOpcoes
                    {
                        InsuranceValue = input.ValorSeguro,
                        UseInsuranceValue = input.ValorSeguro > 0
                    },
                    Package = new CotacaoPacote
                    {
                        Weight = input.Peso,
                        Height = input.Altura,
                        Width = input.Largura,
                        Length = input.Comprimento
                    }
                };

                var resultado = await _superfrete.CotarFreteAsync(request);
                // Filtra resultados com erro
                var sucesso = resultado.Where(r => !r.HasError).ToList();
                return Ok(sucesso);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao cotar frete");
                return StatusCode(502, new { message = ex.Message });
            }
        }

        /// <summary>Cria uma etiqueta de envio no Superfrete.</summary>
        [HttpPost("CriarEtiqueta")]
        public async Task<IActionResult> CriarEtiqueta([FromBody] CriarEtiquetaInput input)
        {
            try
            {
                var request = new CriarEtiquetaRequest
                {
                    Service = input.ServicoId,
                    Platform = "Abrechozeira",
                    From = new EtiquetaEndereco
                    {
                        Name = input.RemetenteNome ?? "",
                        Address = input.RemetenteEndereco ?? "",
                        Number = input.RemetenteNumero ?? "1",
                        District = input.RemetenteBairro ?? "",
                        City = input.RemetenteCidade ?? "",
                        StateAbbr = input.RemetenteEstado ?? "",
                        PostalCode = (input.RemetenteCep ?? "").Replace("-", "")
                    },
                    To = new EtiquetaDestinatario
                    {
                        Name = input.DestinatarioNome,
                        Address = input.DestinatarioEndereco,
                        Number = input.DestinatarioNumero,
                        District = input.DestinatarioBairro,
                        City = input.DestinatarioCidade,
                        StateAbbr = input.DestinatarioEstado,
                        PostalCode = input.DestinatarioCep.Replace("-", ""),
                        Email = input.DestinatarioEmail,
                        Document = input.DestinatarioCpf
                    },
                    Volumes = new CotacaoPacote
                    {
                        Weight = input.Peso,
                        Height = input.Altura,
                        Width = input.Largura,
                        Length = input.Comprimento
                    },
                    Products = input.Produtos.Select(p => new EtiquetaProduto
                    {
                        Name = p.Nome,
                        Quantity = p.Quantidade,
                        UnitaryValue = p.ValorUnitario
                    }).ToList(),
                    Options = new EtiquetaOpcoes
                    {
                        NonCommercial = true // Usa declaração de conteúdo
                    }
                };

                var resultado = await _superfrete.CriarEtiquetaAsync(request);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar etiqueta");
                return StatusCode(502, new { message = ex.Message });
            }
        }

        /// <summary>Lista todas as etiquetas geradas na conta SuperFrete.</summary>
        [HttpGet("Etiquetas")]
        public async Task<IActionResult> ListarEtiquetas()
        {
            try
            {
                var resultado = await _superfrete.ListarEtiquetasAsync();
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar etiquetas");
                return StatusCode(502, new { message = ex.Message });
            }
        }

        /// <summary>Retorna os detalhes de uma etiqueta específica pelo ID.</summary>
        [HttpGet("Etiqueta/{id}")]
        public async Task<IActionResult> ObterEtiqueta(string id)
        {
            try
            {
                var resultado = await _superfrete.ObterEtiquetaAsync(id);
                if (resultado == null) return NotFound();
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter etiqueta {Id}", id);
                return StatusCode(502, new { message = ex.Message });
            }
        }
    }
}
