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
    /// Plano B para os comentarios de live do Instagram: a Meta, por algum motivo
    /// (bug conhecido, relatado por outros devs em forums), simplesmente nao entrega
    /// o webhook do campo "live_comments" em vários casos, mesmo com tudo assinado e
    /// configurado corretamente (confirmado por testes manuais em 05/09/2026: DMs e o
    /// botao de "Teste" chegam, comentarios de live real nao chegam nunca).
    ///
    /// A leitura direta (GET /{live-media-id}/comments) funciona perfeitamente com o
    /// mesmo token, entao esse servico substitui a dependencia do push por um polling:
    /// consulta se ha uma live ativa e, se houver, busca os comentarios periodicamente,
    /// salvando os que ainda nao existem no banco (por InstagramCommentId).
    /// </summary>
    public class InstagramLivePollingService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;
        private readonly ILogger<InstagramLivePollingService> _logger;

        private static readonly TimeSpan IntervaloComLiveAtiva = TimeSpan.FromSeconds(6);
        private static readonly TimeSpan IntervaloSemLive = TimeSpan.FromSeconds(25);
        private const string ApiVersion = "v23.0";

        private long? _liveAtualId = null;
        private HashSet<string> _comentariosConhecidos = new();

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

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var accessTokenInicial = _configuration["Instagram:AccessToken"];
            if (string.IsNullOrWhiteSpace(accessTokenInicial))
            {
                _logger.LogInformation("Instagram:AccessToken nao configurado - polling de comentarios de live desativado.");
                return;
            }

            _logger.LogInformation("Polling de comentarios de live do Instagram iniciado (plano B para o webhook live_comments).");

            while (!stoppingToken.IsCancellationRequested)
            {
                var proximoIntervalo = IntervaloSemLive;

                try
                {
                    // Re-le a cada ciclo, caso o token seja trocado via deploy sem reiniciar o processo.
                    var accessToken = _configuration["Instagram:AccessToken"];
                    if (string.IsNullOrWhiteSpace(accessToken))
                    {
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
                            _logger.LogInformation("Polling detectou live ativa: {LiveId}", liveMediaId);
                        }

                        await BuscarESalvarComentariosAsync(httpClient, accessToken, liveMediaId.Value, stoppingToken);
                        proximoIntervalo = IntervaloComLiveAtiva;
                    }
                    else if (_liveAtualId != null)
                    {
                        await FinalizarLiveAsync(_liveAtualId.Value, stoppingToken);
                        _logger.LogInformation("Polling detectou fim da live: {LiveId}", _liveAtualId);
                        _liveAtualId = null;
                        _comentariosConhecidos = new HashSet<string>();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro no polling de comentarios de live do Instagram.");
                }

                try
                {
                    await Task.Delay(proximoIntervalo, stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    // Encerramento normal do servico (app sendo finalizado).
                }
            }
        }

        private async Task<long?> ObterLiveMediaIdAsync(HttpClient httpClient, string accessToken, CancellationToken ct)
        {
            var url = $"https://graph.instagram.com/{ApiVersion}/me/live_media?fields=id&access_token={Uri.EscapeDataString(accessToken)}";

            using var response = await httpClient.GetAsync(url, ct);
            var json = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Falha ao consultar live_media ({Status}): {Erro}", response.StatusCode, json);
                return null;
            }

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
                _logger.LogInformation("Nova LiveSession criada via polling: {LiveId}", liveVideoId);
            }
            else if (liveSession.EndedAt != null)
            {
                // A mesma live voltou a aparecer como ativa (raro) - reabre a sessao.
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
            var url = $"https://graph.instagram.com/{ApiVersion}/{liveVideoId}/comments?fields=id,text,username,timestamp&access_token={Uri.EscapeDataString(accessToken)}";

            using var response = await httpClient.GetAsync(url, ct);
            var json = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Falha ao consultar comentarios da live {LiveId} ({Status}): {Erro}", liveVideoId, response.StatusCode, json);
                return;
            }

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
                var username = item.TryGetProperty("username", out var userEl) ? (userEl.GetString() ?? "desconhecido") : "desconhecido";
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
                _logger.LogInformation("Comentario salvo via polling: {Username} - {Texto}", c.Username, c.CommentText);
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
