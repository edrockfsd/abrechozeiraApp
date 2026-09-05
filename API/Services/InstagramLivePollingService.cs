using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ABrechozeiraApp.Models;

namespace ABrechozeiraApp.Services
{
    /// <summary>
    /// Serviço em segundo plano para captura em tempo real (1 segundo) dos comentários de Live do Instagram.
    /// Utiliza a Graph API oficial do Instagram (GET /{live-id}/comments) com intervalo adaptativo e
    /// monitoramento de cota (Rate Limit), operando de forma autônoma sem depender do webhook push.
    /// </summary>
    public class InstagramLivePollingService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<InstagramLivePollingService> _logger;

        // Intervalo padrão de 1 segundo durante a Live ativa (solicitado pelo usuário)
        private static readonly TimeSpan IntervaloComLiveAtiva = TimeSpan.FromMilliseconds(1000);
        // Intervalo de segurança caso a cota da Meta atinja 85%
        private static readonly TimeSpan IntervaloThrottling = TimeSpan.FromMilliseconds(2500);
        // Intervalo em repouso (quando não há transmissão ativa)
        private static readonly TimeSpan IntervaloSemLive = TimeSpan.FromSeconds(15);
        private const string ApiVersion = "v23.0";

        private long? _liveAtualId = null;
        private HashSet<string> _comentariosConhecidos = new();
        private bool _emThrottling = false;

        public InstagramLivePollingService(
            IServiceScopeFactory scopeFactory,
            IHttpClientFactory httpClientFactory,
            IConfiguration configuration,
            ILogger<InstagramLivePollingService> logger)
        {
            _scopeFactory = scopeFactory;
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
            _logger = logger;
        }

        private string? ObterTokenInstagram()
        {
            var tokenConfig = _configuration["Instagram:InstagramUserToken"] ?? _configuration["Instagram:AccessToken"];
            if (!string.IsNullOrWhiteSpace(tokenConfig))
            {
                return tokenConfig.Trim();
            }
            return null;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Serviço de captura em tempo real de comentários do Instagram INICIADO (Modo: 1 segundo com proteção de rate limit).");

            while (!stoppingToken.IsCancellationRequested)
            {
                var proximoIntervalo = IntervaloSemLive;

                try
                {
                    var accessToken = ObterTokenInstagram();
                    if (string.IsNullOrWhiteSpace(accessToken))
                    {
                        _logger.LogWarning("Instagram:AccessToken não configurado. Aguardando...");
                        await Task.Delay(IntervaloSemLive, stoppingToken);
                        continue;
                    }

                    var httpClient = _httpClientFactory.CreateClient("InstagramGraph");

                    var liveMediaId = await ObterLiveMediaIdAsync(httpClient, accessToken, stoppingToken);

                    if (liveMediaId != null)
                    {
                        if (_liveAtualId != liveMediaId)
                        {
                            _liveAtualId = liveMediaId;
                            _comentariosConhecidos = await PrepararNovaLiveAsync(liveMediaId.Value, stoppingToken);
                            _logger.LogInformation(">>> LIVE ATIVA DETECTADA! ID: {LiveId}. Modo turbo (1s) ativado.", liveMediaId);
                        }

                        await BuscarESalvarComentariosAsync(httpClient, accessToken, liveMediaId.Value, stoppingToken);

                        proximoIntervalo = _emThrottling ? IntervaloThrottling : IntervaloComLiveAtiva;
                    }
                    else if (_liveAtualId != null)
                    {
                        await FinalizarLiveAsync(_liveAtualId.Value, stoppingToken);
                        _logger.LogInformation("<<< Fim da Live detectado. Live ID: {LiveId}. Voltando para modo repouso (15s).", _liveAtualId);
                        _liveAtualId = null;
                        _comentariosConhecidos.Clear();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro no ciclo de captura de comentários do Instagram.");
                }

                try
                {
                    await Task.Delay(proximoIntervalo, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }
        }

        private async Task<long?> ObterLiveMediaIdAsync(HttpClient httpClient, string accessToken, CancellationToken ct)
        {
            bool isFbToken = accessToken.StartsWith("EAA", StringComparison.OrdinalIgnoreCase);
            var igAccountId = _configuration["Instagram:InstagramAccountId"] ?? "17841472957302808";
            
            var url = isFbToken
                ? $"https://graph.facebook.com/{ApiVersion}/{igAccountId}/live_media?fields=id,status&access_token={Uri.EscapeDataString(accessToken)}"
                : $"https://graph.instagram.com/{ApiVersion}/me/live_media?fields=id,status&access_token={Uri.EscapeDataString(accessToken)}";

            using var response = await httpClient.GetAsync(url, ct);
            VerificarHeadersRateLimit(response);

            if (!response.IsSuccessStatusCode)
            {
                var erro = await response.Content.ReadAsStringAsync(ct);
                _logger.LogWarning("Falha ao consultar live_media ({Status}): {Erro}", response.StatusCode, erro);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(json);

            if (doc.RootElement.TryGetProperty("data", out var data) && data.GetArrayLength() > 0)
            {
                var idStr = data[0].GetProperty("id").GetString();
                if (long.TryParse(idStr, out var id))
                    return id;
            }

            return null;
        }

        private async Task<HashSet<string>> PrepararNovaLiveAsync(long liveVideoId, CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AbrechozeiraContext>();

            var liveSession = await db.LiveSession.FirstOrDefaultAsync(l => l.LiveVideoId == liveVideoId, ct);
            if (liveSession == null)
            {
                liveSession = new LiveSession
                {
                    LiveVideoId = liveVideoId,
                    Status = "live",
                    StartedAt = DateTime.Now
                };
                db.LiveSession.Add(liveSession);
                await db.SaveChangesAsync(ct);
                _logger.LogInformation("Nova LiveSession criada: {LiveId}", liveVideoId);
            }
            else if (liveSession.EndedAt != null)
            {
                liveSession.EndedAt = null;
                liveSession.Status = "live";
                await db.SaveChangesAsync(ct);
            }

            var idsConhecidos = await db.ComentarioLive
                .Where(c => c.LiveSessionId == liveVideoId && c.InstagramCommentId != null)
                .Select(c => c.InstagramCommentId!)
                .ToListAsync(ct);

            return new HashSet<string>(idsConhecidos);
        }

        private async Task BuscarESalvarComentariosAsync(HttpClient httpClient, string accessToken, long liveVideoId, CancellationToken ct)
        {
            bool isFbToken = accessToken.StartsWith("EAA", StringComparison.OrdinalIgnoreCase);
            var baseUrl = isFbToken ? "https://graph.facebook.com" : "https://graph.instagram.com";

            // Requisita id, text, from (com username real) e timestamp
            var url = $"{baseUrl}/{ApiVersion}/{liveVideoId}/comments?fields=id,text,from,timestamp&access_token={Uri.EscapeDataString(accessToken)}";

            using var response = await httpClient.GetAsync(url, ct);
            VerificarHeadersRateLimit(response);

            if (!response.IsSuccessStatusCode)
            {
                var erro = await response.Content.ReadAsStringAsync(ct);
                _logger.LogWarning("Falha ao consultar comentários da live {LiveId} ({Status}): {Erro}", liveVideoId, response.StatusCode, erro);
                return;
            }

            var json = await response.Content.ReadAsStringAsync(ct);
            using var doc = JsonDocument.Parse(json);

            if (!doc.RootElement.TryGetProperty("data", out var data))
                return;

            var novos = new List<ComentarioLive>();

            foreach (var item in data.EnumerateArray())
            {
                var id = item.TryGetProperty("id", out var idEl) ? idEl.GetString() : null;
                if (string.IsNullOrEmpty(id) || _comentariosConhecidos.Contains(id))
                    continue;

                var texto = item.TryGetProperty("text", out var textoEl) ? (textoEl.GetString() ?? "") : "";
                
                // Extrai o username de dentro do objeto 'from' ou da raiz
                var username = "desconhecido";
                if (item.TryGetProperty("from", out var fromEl) && fromEl.TryGetProperty("username", out var userEl))
                {
                    username = userEl.GetString() ?? "desconhecido";
                }
                else if (item.TryGetProperty("username", out var uEl))
                {
                    username = uEl.GetString() ?? "desconhecido";
                }

                var commentTimestamp = DateTime.Now;
                if (item.TryGetProperty("timestamp", out var tsEl) &&
                    DateTimeOffset.TryParse(tsEl.GetString(), out var parsedTs))
                {
                    commentTimestamp = parsedTs.ToLocalTime().DateTime;
                }

                novos.Add(new ComentarioLive
                {
                    Username = username,
                    CommentText = texto,
                    CommentTimestamp = commentTimestamp,
                    CreatedAt = DateTime.Now,
                    LiveSessionId = liveVideoId,
                    InstagramCommentId = id
                });

                _comentariosConhecidos.Add(id);
            }

            if (novos.Count == 0)
                return;

            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AbrechozeiraContext>();
            db.ComentarioLive.AddRange(novos);
            await db.SaveChangesAsync(ct);

            foreach (var c in novos)
            {
                _logger.LogInformation("Comentário salvo em tempo real: [{Username}] -> {Texto}", c.Username, c.CommentText);
            }
        }

        private void VerificarHeadersRateLimit(HttpResponseMessage response)
        {
            try
            {
                if (response.Headers.TryGetValues("X-App-Usage", out var values))
                {
                    var headerVal = values.FirstOrDefault();
                    if (!string.IsNullOrEmpty(headerVal))
                    {
                        using var doc = JsonDocument.Parse(headerVal);
                        if (doc.RootElement.TryGetProperty("call_count", out var cc) && cc.TryGetInt32(out var callCount))
                        {
                            if (callCount >= 85)
                            {
                                if (!_emThrottling)
                                {
                                    _logger.LogWarning("Uso de cota da Meta atingiu {Uso}%. Ativando throttling preventivo (2.5s).", callCount);
                                    _emThrottling = true;
                                }
                            }
                            else if (callCount < 70 && _emThrottling)
                            {
                                _logger.LogInformation("Uso de cota da Meta normalizou em {Uso}%. Retomando modo 1 segundo.", callCount);
                                _emThrottling = false;
                            }
                        }
                    }
                }
            }
            catch
            {
                // Ignora erros na leitura de headers para não travar o loop principal
            }
        }

        private async Task FinalizarLiveAsync(long liveVideoId, CancellationToken ct)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AbrechozeiraContext>();

            var liveSession = await db.LiveSession.FirstOrDefaultAsync(l => l.LiveVideoId == liveVideoId, ct);
            if (liveSession != null && liveSession.EndedAt == null)
            {
                liveSession.EndedAt = DateTime.UtcNow;
                liveSession.Status = "ended";
                await db.SaveChangesAsync(ct);
            }
        }
    }
}
