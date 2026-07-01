using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ABrechozeiraApp.Services
{
    public class WhatsAppService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;
        private readonly ILogger<WhatsAppService> _logger;

        public WhatsAppService(IHttpClientFactory httpClientFactory, IConfiguration config, ILogger<WhatsAppService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
            _logger = logger;
        }

        private HttpClient CriarCliente()
        {
            var client = _httpClientFactory.CreateClient("WhatsAppCloud");
            var token = _config["WhatsApp:ApiToken"] ?? "";
            var baseUrl = _config["WhatsApp:BaseUrl"] ?? "https://graph.facebook.com/v19.0";
            
            client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return client;
        }

        public async Task<bool> SendTemplateMessageAsync(string toPhoneNumber, string templateName, List<string> bodyParameters, string buttonUrlParameter)
        {
            try
            {
                // Limpa o número de telefone (apenas dígitos)
                var cleanPhone = new string(toPhoneNumber.Where(char.IsDigit).ToArray());
                
                // Garante que tem DDI (55 para Brasil). Se começar com 0, remove.
                if (cleanPhone.StartsWith("0"))
                {
                    cleanPhone = cleanPhone.Substring(1);
                }
                if (!cleanPhone.StartsWith("55") && cleanPhone.Length <= 11)
                {
                    cleanPhone = "55" + cleanPhone;
                }

                var client = CriarCliente();
                var phoneId = _config["WhatsApp:PhoneId"] ?? "";

                if (string.IsNullOrWhiteSpace(phoneId))
                {
                    _logger.LogError("ID do telefone da Meta (WhatsApp:PhoneId) não está configurado.");
                    return false;
                }

                var componentsList = new List<object>();

                // Componente do corpo (variáveis {{1}}, {{2}}, etc.)
                if (bodyParameters != null && bodyParameters.Count > 0)
                {
                    var bodyParams = new List<object>();
                    foreach (var param in bodyParameters)
                    {
                        bodyParams.Add(new { type = "text", text = param });
                    }
                    componentsList.Add(new
                    {
                        type = "body",
                        parameters = bodyParams
                    });
                }

                // Componente de botão (URL dinâmica - Call to Action)
                if (!string.IsNullOrWhiteSpace(buttonUrlParameter))
                {
                    componentsList.Add(new
                    {
                        type = "button",
                        sub_type = "url",
                        index = "0", // Primeiro botão cadastrado no template
                        parameters = new[]
                        {
                            new { type = "text", text = buttonUrlParameter }
                        }
                    });
                }

                var payload = new
                {
                    messaging_product = "whatsapp",
                    to = cleanPhone,
                    type = "template",
                    template = new
                    {
                        name = templateName,
                        language = new { code = "pt_BR" },
                        components = componentsList
                    }
                };

                var jsonPayload = JsonSerializer.Serialize(payload);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                var response = await client.PostAsync($"{phoneId}/messages", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    _logger.LogError("Falha ao enviar mensagem de template do WhatsApp. Status: {Status}, Resposta: {Res}", response.StatusCode, errorResponse);
                    return false;
                }

                _logger.LogInformation("Mensagem do WhatsApp via Template '{Template}' enviada com sucesso para {To}", templateName, cleanPhone);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro de exceção ao enviar mensagem de template do WhatsApp.");
                return false;
            }
        }
    }
}
