using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ABrechozeiraApp.Services
{
    // ─── DTOs de Cotação ────────────────────────────────────────────────────────
    public class CotacaoFreteRequest
    {
        [JsonPropertyName("from")]
        public CotacaoEndereco From { get; set; } = new();

        [JsonPropertyName("to")]
        public CotacaoEndereco To { get; set; } = new();

        [JsonPropertyName("services")]
        public string Services { get; set; } = "1,2,17,3";

        [JsonPropertyName("options")]
        public CotacaoOpcoes Options { get; set; } = new();

        [JsonPropertyName("package")]
        public CotacaoPacote? Package { get; set; }
    }

    public class CotacaoEndereco
    {
        [JsonPropertyName("postal_code")]
        public string PostalCode { get; set; } = "";
    }

    public class CotacaoOpcoes
    {
        [JsonPropertyName("own_hand")]
        public bool OwnHand { get; set; } = false;

        [JsonPropertyName("receipt")]
        public bool Receipt { get; set; } = false;

        [JsonPropertyName("insurance_value")]
        public decimal InsuranceValue { get; set; } = 0;

        [JsonPropertyName("use_insurance_value")]
        public bool UseInsuranceValue { get; set; } = false;
    }

    public class CotacaoPacote
    {
        [JsonPropertyName("height")]
        public decimal Height { get; set; }

        [JsonPropertyName("width")]
        public decimal Width { get; set; }

        [JsonPropertyName("length")]
        public decimal Length { get; set; }

        [JsonPropertyName("weight")]
        public decimal Weight { get; set; }
    }

    public class CotacaoFreteResultado
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [JsonPropertyName("discount")]
        public string Discount { get; set; } = "0";

        [JsonPropertyName("currency")]
        public string Currency { get; set; } = "R$";

        [JsonPropertyName("delivery_time")]
        public int DeliveryTime { get; set; }

        [JsonPropertyName("delivery_range")]
        public DeliveryRange? DeliveryRange { get; set; }

        [JsonPropertyName("packages")]
        public List<PacoteResultado>? Packages { get; set; }

        [JsonPropertyName("company")]
        public EmpresaResultado? Company { get; set; }

        [JsonPropertyName("has_error")]
        public bool HasError { get; set; }
    }

    public class DeliveryRange
    {
        [JsonPropertyName("min")]
        public int Min { get; set; }

        [JsonPropertyName("max")]
        public int Max { get; set; }
    }

    public class PacoteResultado
    {
        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [JsonPropertyName("format")]
        public string Format { get; set; } = "";

        [JsonPropertyName("dimensions")]
        public DimensoesResultado? Dimensions { get; set; }

        [JsonPropertyName("weight")]
        public string Weight { get; set; } = "";
    }

    public class DimensoesResultado
    {
        [JsonPropertyName("height")]
        public string Height { get; set; } = "";

        [JsonPropertyName("width")]
        public string Width { get; set; } = "";

        [JsonPropertyName("length")]
        public string Length { get; set; } = "";
    }

    public class EmpresaResultado
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("picture")]
        public string Picture { get; set; } = "";
    }

    // ─── DTOs de Criação de Etiqueta ─────────────────────────────────────────────
    public class CriarEtiquetaRequest
    {
        [JsonPropertyName("from")]
        public EtiquetaEndereco From { get; set; } = new();

        [JsonPropertyName("to")]
        public EtiquetaDestinatario To { get; set; } = new();

        [JsonPropertyName("service")]
        public int Service { get; set; }

        [JsonPropertyName("products")]
        public List<EtiquetaProduto>? Products { get; set; }

        [JsonPropertyName("volumes")]
        public CotacaoPacote Volumes { get; set; } = new();

        [JsonPropertyName("options")]
        public EtiquetaOpcoes? Options { get; set; }

        [JsonPropertyName("platform")]
        public string Platform { get; set; } = "Abrechozeira";
    }

    public class EtiquetaEndereco
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("address")]
        public string Address { get; set; } = "";

        [JsonPropertyName("number")]
        public string Number { get; set; } = "";

        [JsonPropertyName("complement")]
        public string? Complement { get; set; }

        [JsonPropertyName("district")]
        public string District { get; set; } = "";

        [JsonPropertyName("city")]
        public string City { get; set; } = "";

        [JsonPropertyName("state_abbr")]
        public string StateAbbr { get; set; } = "";

        [JsonPropertyName("postal_code")]
        public string PostalCode { get; set; } = "";

        [JsonPropertyName("document")]
        public string? Document { get; set; }
    }

    public class EtiquetaDestinatario : EtiquetaEndereco
    {
        [JsonPropertyName("email")]
        public string? Email { get; set; }
    }

    public class EtiquetaProduto
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; } = 1;

        [JsonPropertyName("unitary_value")]
        public decimal UnitaryValue { get; set; }
    }

    public class EtiquetaOpcoes
    {
        [JsonPropertyName("insurance_value")]
        public decimal? InsuranceValue { get; set; }

        [JsonPropertyName("receipt")]
        public bool Receipt { get; set; } = false;

        [JsonPropertyName("own_hand")]
        public bool OwnHand { get; set; } = false;

        [JsonPropertyName("non_commercial")]
        public bool NonCommercial { get; set; } = true;
    }

    public class CriarEtiquetaResultado
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = "";

        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = "";
    }

    // ─── DTOs de Listagem de Etiquetas ───────────────────────────────────────────
    public class EtiquetaInfo
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = "";

        [JsonPropertyName("protocol")]
        public string? Protocol { get; set; }

        [JsonPropertyName("service_id")]
        public int ServiceId { get; set; }

        [JsonPropertyName("service_name")]
        public string ServiceName { get; set; } = "";

        [JsonPropertyName("tracking")]
        public string? Tracking { get; set; }

        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = "";

        [JsonPropertyName("created_at")]
        public string? CreatedAt { get; set; }

        [JsonPropertyName("destinatario")]
        public string? Destinatario { get; set; }
    }

    // ─── Service ──────────────────────────────────────────────────────────────────
    public class SuperfreteService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _config;
        private readonly ILogger<SuperfreteService> _logger;

        public SuperfreteService(IHttpClientFactory httpClientFactory, IConfiguration config, ILogger<SuperfreteService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _config = config;
            _logger = logger;
        }

        private HttpClient CriarCliente()
        {
            var client = _httpClientFactory.CreateClient("Superfrete");
            var token = _config["Superfrete:Token"] ?? "";
            var baseUrl = _config["Superfrete:BaseUrl"] ?? "https://sandbox.superfrete.com";
            client.BaseAddress = new Uri(baseUrl);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Abrechozeira/1.0 (integracao@abrechozeira.com.br)");
            return client;
        }

        private StringContent JsonContent(object obj)
        {
            var json = JsonSerializer.Serialize(obj, new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull });
            return new StringContent(json, Encoding.UTF8, "application/json");
        }

        /// <summary>Calcula cotações de frete via API SuperFrete.</summary>
        public async Task<List<CotacaoFreteResultado>> CotarFreteAsync(CotacaoFreteRequest request)
        {
            // Preenche o CEP de origem com o do remetente padrão se não informado
            if (string.IsNullOrWhiteSpace(request.From.PostalCode))
                request.From.PostalCode = (_config["Superfrete:Remetente:CEP"] ?? "").Replace("-", "");

            var client = CriarCliente();
            var response = await client.PostAsync("/api/v0/calculator", JsonContent(request));

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Erro Superfrete CotarFrete: {Status} — {Body}", response.StatusCode, error);
                throw new Exception($"Erro ao cotar frete: {response.StatusCode}");
            }

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<CotacaoFreteResultado>>(json) ?? new();
        }

        /// <summary>Cria uma etiqueta de envio no Superfrete (status inicial: pending).</summary>
        public async Task<CriarEtiquetaResultado> CriarEtiquetaAsync(CriarEtiquetaRequest request)
        {
            // Preenche remetente padrão se não informado
            if (string.IsNullOrWhiteSpace(request.From.Name))
            {
                request.From.Name = _config["Superfrete:Remetente:Nome"] ?? "Abrechozeira Loja";
                request.From.Address = _config["Superfrete:Remetente:Endereco"] ?? "";
                request.From.Number = _config["Superfrete:Remetente:Numero"] ?? "1";
                request.From.Complement = _config["Superfrete:Remetente:Complemento"];
                request.From.District = _config["Superfrete:Remetente:Bairro"] ?? "";
                request.From.City = _config["Superfrete:Remetente:Cidade"] ?? "";
                request.From.StateAbbr = _config["Superfrete:Remetente:Estado"] ?? "";
                request.From.PostalCode = (_config["Superfrete:Remetente:CEP"] ?? "").Replace("-", "");
            }

            var client = CriarCliente();
            var response = await client.PostAsync("/api/v0/cart", JsonContent(request));

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Erro Superfrete CriarEtiqueta: {Status} — {Body}", response.StatusCode, error);
                throw new Exception($"Erro ao criar etiqueta: {response.StatusCode} — {error}");
            }

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<CriarEtiquetaResultado>(json) ?? new();
        }

        /// <summary>Lista todas as etiquetas geradas na conta SuperFrete.</summary>
        public async Task<List<EtiquetaInfo>> ListarEtiquetasAsync()
        {
            var client = CriarCliente();
            var response = await client.GetAsync("/api/v0/me/orders?limit=100&per_page=100");

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Erro Superfrete ListarEtiquetas: {Status} — {Body}", response.StatusCode, error);
                throw new Exception($"Erro ao listar etiquetas: {response.StatusCode}");
            }

            var json = await response.Content.ReadAsStringAsync();
            var lista = new List<EtiquetaInfo>();

            try
            {
                using var doc = JsonDocument.Parse(json);
                JsonElement dataElement;

                if (doc.RootElement.ValueKind == JsonValueKind.Array)
                {
                    dataElement = doc.RootElement;
                }
                else if (doc.RootElement.TryGetProperty("data", out var dataEl))
                {
                    dataElement = dataEl;
                }
                else if (doc.RootElement.TryGetProperty("orders", out var ordersEl))
                {
                    dataElement = ordersEl;
                }
                else
                {
                    return lista;
                }

                if (dataElement.ValueKind == JsonValueKind.Array)
                {
                    foreach (var item in dataElement.EnumerateArray())
                    {
                        var etiqueta = new EtiquetaInfo();
                        
                        // ID / Order ID
                        if (item.TryGetProperty("order_id", out var orderIdEl))
                            etiqueta.Id = orderIdEl.GetString() ?? "";
                        else if (item.TryGetProperty("id", out var idEl))
                            etiqueta.Id = idEl.GetString() ?? "";

                        // Protocol
                        if (item.TryGetProperty("protocol", out var protoEl))
                            etiqueta.Protocol = protoEl.GetString();

                        // Tracking
                        if (item.TryGetProperty("tracking", out var trackEl))
                            etiqueta.Tracking = trackEl.GetString();

                        // Status
                        if (item.TryGetProperty("status", out var statusEl))
                            etiqueta.Status = statusEl.GetString() ?? "";

                        // CreatedAt
                        if (item.TryGetProperty("created_at", out var createdEl))
                            etiqueta.CreatedAt = createdEl.GetString();

                        // Destinatario (from to.name)
                        if (item.TryGetProperty("to", out var toEl) && toEl.ValueKind == JsonValueKind.Object)
                        {
                            if (toEl.TryGetProperty("name", out var toNameEl))
                            {
                                etiqueta.Destinatario = toNameEl.GetString() ?? "";
                            }
                        }

                        // ServiceName / Carrier
                        if (item.TryGetProperty("service_name", out var serviceNameEl))
                        {
                            etiqueta.ServiceName = serviceNameEl.GetString() ?? "";
                        }
                        else if (item.TryGetProperty("carrier", out var carrierEl))
                        {
                            etiqueta.ServiceName = carrierEl.GetString() ?? "";
                        }

                        // Price (nested price.total or flat price)
                        if (item.TryGetProperty("price", out var priceEl))
                        {
                            if (priceEl.ValueKind == JsonValueKind.Object && priceEl.TryGetProperty("total", out var totalEl))
                            {
                                if (totalEl.ValueKind == JsonValueKind.Number)
                                    etiqueta.Price = totalEl.GetDecimal();
                            }
                            else if (priceEl.ValueKind == JsonValueKind.Number)
                            {
                                etiqueta.Price = priceEl.GetDecimal();
                            }
                        }

                        lista.Add(etiqueta);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao analisar resposta JSON do Superfrete");
            }

            return lista;
        }

        /// <summary>Retorna informações detalhadas de uma etiqueta pelo ID.</summary>
        public async Task<EtiquetaInfo?> ObterEtiquetaAsync(string id)
        {
            var client = CriarCliente();
            var response = await client.GetAsync($"/api/v0/order/info/{id}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Superfrete ObterEtiqueta {Id}: {Status}", id, response.StatusCode);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<EtiquetaInfo>(json);
        }
    }
}
