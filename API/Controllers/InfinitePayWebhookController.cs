using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using System.Text.Json.Serialization;
using ABrechozeiraApp.Services;
using ABrechozeiraApp.Models;

namespace ABrechozeiraApp.Controllers
{
    [ApiController]
    [Route("api/webhooks/infinitepay")]
    public class InfinitePayWebhookController : ControllerBase
    {
        private readonly InfinitePayService _infinitePay;
        private readonly PagamentoFreteService _pagamentoFrete;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<InfinitePayWebhookController> _logger;
        private readonly AbrechozeiraContext _context;

        public InfinitePayWebhookController(
            InfinitePayService infinitePay,
            PagamentoFreteService pagamentoFrete,
            IWebHostEnvironment env,
            ILogger<InfinitePayWebhookController> logger,
            AbrechozeiraContext context)
        {
            _infinitePay = infinitePay;
            _pagamentoFrete = pagamentoFrete;
            _env = env;
            _logger = logger;
            _context = context;
        }

        private EnvioLoteMap? ObterMapeamentoPorTransacao(string transacaoId)
        {
            try
            {
                return _context.EnvioLoteMap.FirstOrDefault(x => x.TransacaoId == transacaoId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao ler mapeamento do PIX para etiqueta");
            }
            return null;
        }

        [HttpPost]
        public async Task<IActionResult> ReceiveWebhook([FromBody] JsonElement body)
        {
            _logger.LogInformation("Recebido Webhook da InfinitePay: {Body}", body.GetRawText());

            try
            {
                string slug = string.Empty;
                if (body.TryGetProperty("invoice_slug", out var invoiceSlugEl))
                    slug = invoiceSlugEl.GetString() ?? "";
                else if (body.TryGetProperty("slug", out var slugEl))
                    slug = slugEl.GetString() ?? "";

                if (!body.TryGetProperty("order_nsu", out var orderNsuEl) ||
                    !body.TryGetProperty("transaction_nsu", out var transactionNsuEl) || 
                    string.IsNullOrEmpty(slug))
                {
                    _logger.LogWarning("Webhook inválido ou incompleto da InfinitePay.");
                    return BadRequest("Missing fields");
                }

                var orderNsu = orderNsuEl.GetString();
                var transactionNsu = transactionNsuEl.GetString();

                if (string.IsNullOrEmpty(slug) || string.IsNullOrEmpty(orderNsu) || string.IsNullOrEmpty(transactionNsu))
                    return BadRequest("Invalid fields");

                bool isPaid;
                if (transactionNsu == "teste123" && _env.IsDevelopment())
                {
                    isPaid = true; // Bypass apenas para teste local (em produção qualquer um poderia marcar como pago)
                }
                else
                {
                    isPaid = await _infinitePay.VerificarPagamentoAsync(transactionNsu, orderNsu, slug);
                }

                if (!isPaid)
                {
                    _logger.LogWarning("Transação InfinitePay {TransactionNsu} não foi confirmada como paga.", transactionNsu);
                    return Ok();
                }

                _logger.LogInformation("Pagamento do frete confirmado! OrderNsu (TransacaoId): {OrderNsu}", orderNsu);

                var mapInfo = ObterMapeamentoPorTransacao(orderNsu);
                if (mapInfo == null)
                {
                    _logger.LogWarning("Transação {OrderNsu} paga, mas não existe cotação com esse TransacaoId.", orderNsu);
                    return Ok();
                }

                // Registra o pagamento mesmo sem etiqueta: a cliente pode pagar antes da etiqueta ser gerada.
                // Nesse caso a etiqueta é liberada no Superfrete assim que for gerada.
                await _pagamentoFrete.ConfirmarPagamentoAsync(mapInfo, PagamentoFreteService.OrigemAutomatico);

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro interno ao processar webhook da InfinitePay");
                return StatusCode(500);
            }
        }
    }
}
