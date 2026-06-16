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
        private readonly SuperfreteService _superfrete;
        private readonly EmailService _emailService;
        private readonly ILogger<InfinitePayWebhookController> _logger;
        private readonly AbrechozeiraContext _context;

        public InfinitePayWebhookController(
            InfinitePayService infinitePay,
            SuperfreteService superfrete,
            EmailService emailService,
            ILogger<InfinitePayWebhookController> logger,
            AbrechozeiraContext context)
        {
            _infinitePay = infinitePay;
            _superfrete = superfrete;
            _emailService = emailService;
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

        private void SalvarMapeamento(string transacaoId, EnvioLoteMap info)
        {
            try
            {
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao salvar mapeamento atualizado");
            }
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
                if (transactionNsu == "teste123")
                {
                    isPaid = true; // Bypass para teste local
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
                if (mapInfo == null || string.IsNullOrEmpty(mapInfo.EtiquetaId))
                {
                    _logger.LogWarning("Transação {OrderNsu} paga, mas não há mapeamento para EtiquetaId do Superfrete.", orderNsu);
                    return Ok();
                }

                // Atualizar StatusPagamento para Pago
                mapInfo.StatusPagamento = "Pago";
                SalvarMapeamento(orderNsu, mapInfo);

                var sucessoCheckout = await _superfrete.CheckoutAsync(mapInfo.EtiquetaId);
                if (sucessoCheckout)
                {
                    _logger.LogInformation("Checkout da etiqueta {EtiquetaId} realizado com sucesso!", mapInfo.EtiquetaId);
                    
                    // Atualizar StatusSuperfrete para Liberada
                    mapInfo.StatusSuperfrete = "Liberada";
                    SalvarMapeamento(orderNsu, mapInfo);

                    var factory = HttpContext.RequestServices.GetRequiredService<IServiceScopeFactory>();
                    _ = Task.Run(async () =>
                    {
                        using var scope = factory.CreateScope();
                        var superfrete = scope.ServiceProvider.GetRequiredService<SuperfreteService>();
                        var emailService = scope.ServiceProvider.GetRequiredService<EmailService>();
                        var logger = scope.ServiceProvider.GetRequiredService<ILogger<InfinitePayWebhookController>>();

                        try
                        {
                            await Task.Delay(3000); 
                            var info = await superfrete.ObterEtiquetaAsync(mapInfo.EtiquetaId);
                            if (info != null && !string.IsNullOrWhiteSpace(info.Tracking) && !string.IsNullOrWhiteSpace(mapInfo.Email))
                            {
                                await emailService.EnviarRastreioAsync(mapInfo.Email, mapInfo.Nome, info.Tracking, info.ServiceName);
                                logger.LogInformation("E-mail de rastreio automático enviado para {Email}.", mapInfo.Email);
                            }
                        }
                        catch (Exception ex)
                        {
                            logger.LogError(ex, "Erro ao enviar e-mail de rastreio pós-checkout.");
                        }
                    });
                }
                else
                {
                    _logger.LogError("Erro ao processar checkout da etiqueta {EtiquetaId} no Superfrete após pagamento.", mapInfo.EtiquetaId);
                    
                    // Atualizar StatusSuperfrete para Erro
                    mapInfo.StatusSuperfrete = "Erro Checkout";
                    SalvarMapeamento(orderNsu, mapInfo);
                }

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
