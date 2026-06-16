using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ABrechozeiraApp.Services
{
    public class InfinitePayService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<InfinitePayService> _logger;

        public InfinitePayService(IConfiguration config, ILogger<InfinitePayService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task<string> GerarLinkCheckoutAsync(decimal valor, string descricao, string orderNsu, string customerName, string customerEmail)
        {
            try
            {
                var handle = _config["InfinitePay:InfiniteTag"];
                if (string.IsNullOrEmpty(handle))
                    throw new Exception("InfiniteTag não configurada no appsettings.json.");

                var webhookUrl = _config["InfinitePay:WebhookUrl"];

                // Converter valor para centavos
                var valorCentavos = (int)(valor * 100);

                var payload = new
                {
                    handle = handle,
                    webhook_url = webhookUrl,
                    order_nsu = orderNsu,
                    items = new[]
                    {
                        new
                        {
                            quantity = 1,
                            price = valorCentavos,
                            description = descricao
                        }
                    }
                };

                using var client = new HttpClient();
                var content = new StringContent(JsonSerializer.Serialize(payload, new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }), Encoding.UTF8, "application/json");

                var response = await client.PostAsync("https://api.checkout.infinitepay.io/links", content);
                var responseString = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError("Erro InfinitePay GerarLink: {Status} - {Body}", response.StatusCode, responseString);
                    throw new Exception($"Erro InfinitePay: {responseString}");
                }

                using var doc = JsonDocument.Parse(responseString);
                if (doc.RootElement.TryGetProperty("url", out var urlElement))
                {
                    return urlElement.GetString() ?? string.Empty;
                }

                throw new Exception("URL não retornada pela InfinitePay.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao gerar link de pagamento na InfinitePay.");
                throw;
            }
        }

        public async Task<bool> VerificarPagamentoAsync(string transactionNsu, string orderNsu, string slug)
        {
            try
            {
                var handle = _config["InfinitePay:InfiniteTag"];
                var payload = new
                {
                    handle = handle,
                    order_nsu = orderNsu,
                    transaction_nsu = transactionNsu,
                    slug = slug
                };

                using var client = new HttpClient();
                var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

                var response = await client.PostAsync("https://api.checkout.infinitepay.io/payment_check", content);
                var responseString = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    using var doc = JsonDocument.Parse(responseString);
                    if (doc.RootElement.TryGetProperty("paid", out var paidProp) && paidProp.GetBoolean())
                    {
                        return true;
                    }
                }
                else
                {
                    _logger.LogWarning("Falha ao verificar pagamento na InfinitePay: {Status} - {Body}", response.StatusCode, responseString);
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar pagamento no payment_check.");
                return false;
            }
        }
    }
}
