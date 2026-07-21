using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
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
            var token = _config["WhatsApp:ApiToken"] ?? "";
            var baseUrl = (_config["WhatsApp:BaseUrl"] ?? "https://graph.facebook.com/v23.0").TrimEnd('/') + "/";

            _logger.LogInformation("[WhatsApp DEBUG] Token primeiros 30 chars: {T} | BaseUrl: {U}", 
                token.Length > 30 ? token.Substring(0, 30) : token, baseUrl);

            var client = new HttpClient();
            client.BaseAddress = new Uri(baseUrl);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            return client;
        }

        /// <summary>
        /// Normaliza o número de telefone brasileiro para o padrão exigido pela WhatsApp Cloud API.
        /// O WhatsApp ID de celulares brasileiros fora dos DDDs 11-19, 21, 22, 24, 27-28 NÃO tem o nono dígito.
        /// Esta função remove o 9 apenas nos DDDs afetados (ex: 5541985130777 -> 554185130777).
        /// </summary>
        public static string NormalizarNumeroBrasil(string numero)
        {
            if (string.IsNullOrWhiteSpace(numero)) return "";

            var d = new string(numero.Where(char.IsDigit).ToArray());

            if (d.StartsWith("0")) d = d.Substring(1);
            if (!d.StartsWith("55")) d = "55" + d;

            if (d.Length != 13) return d; // Já em 12 dígitos ou formato não-padrão

            if (int.TryParse(d.Substring(2, 2), out int ddd))
            {
                bool mantemNove = (ddd >= 11 && ddd <= 19) || ddd == 21 || ddd == 22
                               || ddd == 24 || ddd == 27 || ddd == 28;

                if (!mantemNove && d[4] == '9')
                {
                    return d.Remove(4, 1);
                }
            }

            return d;
        }

        /// <summary>
        /// Envia uma mensagem baseada em template cadastrado na Meta Cloud API.
        /// </summary>
        public async Task<bool> SendTemplateMessageAsync(
            string toPhoneNumber, 
            string templateName, 
            List<string> bodyParameters, 
            string buttonUrlParameter, 
            string languageCode = "pt_BR")
        {
            try
            {
                var cleanPhone = NormalizarNumeroBrasil(toPhoneNumber);
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
                    var bodyParams = bodyParameters.Select(param => new { type = "text", text = param }).ToList<object>();
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
                        language = new { code = languageCode },
                        components = componentsList.Count > 0 ? componentsList : null
                    }
                };

                var jsonOptions = new JsonSerializerOptions
                {
                    DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
                };
                var jsonPayload = JsonSerializer.Serialize(payload, jsonOptions);
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

        /// <summary>
        /// cotacao_frete_v2 — cotação de frete com link de pagamento.
        /// </summary>
        public Task<bool> EnviarCotacaoFreteAsync(
            string destino,
            string nomeCliente,
            decimal valorFrete,
            string modalidade,
            string checkoutSlug)
        {
            var template = _config["WhatsApp:TemplateCotacao"] ?? "cotacao_frete_v2";
            var lang = _config["WhatsApp:TemplateCotacaoLanguage"] ?? "pt_BR";

            var bodyParams = new List<string>
            {
                nomeCliente,
                valorFrete.ToString("0.00", CultureInfo.InvariantCulture),
                modalidade
            };

            return SendTemplateMessageAsync(destino, template, bodyParams, checkoutSlug, lang);
        }

        /// <summary>
        /// rastreio_envio_v2 — aviso de postagem com rastreio dos Correios.
        /// </summary>
        public Task<bool> EnviarRastreioEnvioAsync(
            string destino,
            string nomeCliente,
            string transportadora,
            string codigoRastreio)
        {
            var template = _config["WhatsApp:TemplateRastreio"] ?? "rastreio_envio_v2";
            var lang = _config["WhatsApp:TemplateRastreioLanguage"] ?? "pt_BR";

            var bodyParams = new List<string>
            {
                nomeCliente,
                transportadora,
                codigoRastreio
            };

            return SendTemplateMessageAsync(destino, template, bodyParams, codigoRastreio, lang);
        }

        /// <summary>
        /// resumo_compra_v1 — resumo do pedido com link de pagamento.
        /// </summary>
        public Task<bool> EnviarResumoCompraAsync(
            string destino,
            string nomeCliente,
            string numeroPedido,
            string itens,
            decimal valorTotal,
            string checkoutSlug)
        {
            var template = _config["WhatsApp:TemplateResumoCompra"] ?? "resumo_compra_v1";
            var lang = _config["WhatsApp:TemplateResumoCompraLanguage"] ?? "pt_BR";

            var bodyParams = new List<string>
            {
                nomeCliente,
                numeroPedido,
                itens,
                valorTotal.ToString("0.00", CultureInfo.InvariantCulture)
            };

            return SendTemplateMessageAsync(destino, template, bodyParams, checkoutSlug, lang);
        }

        /// <summary>
        /// hello_world — template de teste da Meta.
        /// </summary>
        public Task<bool> EnviarHelloWorldAsync(string destino)
        {
            var template = _config["WhatsApp:TemplateHelloWorld"] ?? "hello_world";
            return SendTemplateMessageAsync(destino, template, new List<string>(), "", "en_US");
        }
    }
}
