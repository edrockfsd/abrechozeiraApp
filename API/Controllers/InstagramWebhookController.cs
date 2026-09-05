using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ABrechozeiraApp.Models;
using System.Text.Json;
using System.IO;
using System.Text.Json.Serialization;
using System.Net.Http.Json;


namespace ABrechozeiraApp.Controllers
{
    // Classes para deserialização dos payloads do Instagram
    public class InstagramWebhookPayload
    {
        public string field { get; set; } = string.Empty;
        public LiveCommentValue value { get; set; } = null!;
    }

    public class LiveVideoValue
    {
        public string id { get; set; } = string.Empty;
        public string status { get; set; } = string.Empty;
    }

    public class LiveCommentValue
    {
        public From from { get; set; } = null!;
        public Media media { get; set; } = null!;
        public string id { get; set; } = string.Empty;
        public string text { get; set; } = string.Empty;
    }

    public class From
    {
        public string id { get; set; } = string.Empty;
        public string username { get; set; } = string.Empty;
        public string self_ig_scoped_id { get; set; } = string.Empty;
    }

    public class Media
    {
        public string id { get; set; } = string.Empty;
        public string media_product_type { get; set; } = string.Empty;
    }

    // Classes para deserialização do payload real do Instagram
    public class InstagramWebhookRoot
    {
        public string @object { get; set; }
        public List<InstagramEntry> entry { get; set; }
    }

    public class InstagramEntry
    {
        public string id { get; set; }
        public long time { get; set; }
        public List<InstagramChange> changes { get; set; }
    }

    public class InstagramChange
    {
        public string field { get; set; }
        public JsonElement value { get; set; }
    }

    // Em InstagramWebhookController.cs
    [Route("api/instagram-webhook")]
    [ApiController]
    public class InstagramWebhookController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly AbrechozeiraContext _dbContext;

        // DEBUG TEMPORARIO - guarda em memoria o ultimo payload cru recebido e qualquer
        // erro de processamento, so para diagnostico. Remover assim que resolvermos.
        private static string? _ultimoPayloadRecebidoDebug = null;
        private static string? _ultimoErroDebug = null;
        private static DateTime? _ultimoRecebidoEmDebug = null;

        // Injetar IConfiguration para ler o Verify Token do appsettings.json
        public InstagramWebhookController(IConfiguration configuration, AbrechozeiraContext dbContext)
        {
            _configuration = configuration;
            _dbContext = dbContext;
        }

        // Ação para verificação do Webhook (GET)
        [HttpGet]
        public IActionResult VerifyWebhook([FromQuery(Name = "hub.mode")] string mode,
                                           [FromQuery(Name = "hub.verify_token")] string token,
                                           [FromQuery(Name = "hub.challenge")] string challenge)
        {
            var verifyToken = _configuration["Instagram:VerifyToken"]; // Pegar do appsettings.json

            if (mode == "subscribe" && token == verifyToken)
            {
                Console.WriteLine("WEBHOOK VERIFICADO COM SUCESSO!");
                return Ok(challenge); // Retorna o 'challenge' com status 200 OK
            }
            else
            {
                Console.WriteLine("FALHA NA VERIFICAÇÃO DO WEBHOOK.");
                return Forbid(); // Retorna 403 Forbidden
            }
        }

        // ENDPOINT TEMPORARIO DE DEBUG - remover apos validar o recebimento de eventos de teste da Meta.
        // Lista os ultimos comentarios salvos direto da tabela ComentarioLive, sem depender de LiveSession existir.
        [HttpGet("debug-comentarios")]
        public async Task<IActionResult> DebugComentarios()
        {
            var ultimos = await _dbContext.ComentarioLive
                .OrderByDescending(c => c.CreatedAt)
                .Take(10)
                .Select(c => new
                {
                    c.Id,
                    c.Username,
                    c.CommentText,
                    c.CommentTimestamp,
                    c.CreatedAt,
                    c.LiveSessionId,
                    c.InstagramCommentId
                })
                .ToListAsync();

            return Ok(ultimos);
        }

        // Ação para receber os eventos (POST)
        [HttpPost]
        public async Task<IActionResult> ReceiveWebhookEvent()
        {
            try
            {
                using var reader = new StreamReader(Request.Body);
                var body = await reader.ReadToEndAsync();                

                Console.WriteLine("--- NOVO EVENTO RECEBIDO ---");
                Console.WriteLine(body);

                // DEBUG TEMPORARIO
                _ultimoPayloadRecebidoDebug = body;
                _ultimoRecebidoEmDebug = DateTime.Now;
                _ultimoErroDebug = null;

                var root = JsonSerializer.Deserialize<InstagramWebhookRoot>(body);

                if (root?.entry != null)
                {
                    foreach (var entry in root.entry)
                    {
                        // Nem todo evento e uma "mudanca de campo" (comments/live_comments/etc):
                        // eventos de mensagens (DM) chegam com "messaging" em vez de "changes",
                        // entao entry.changes vem nulo nesses casos - so processamos se existir.
                        if (entry.changes == null)
                        {
                            Console.WriteLine("Evento recebido sem 'changes' (provavelmente mensagem/DM) - ignorado por este endpoint.");
                            continue;
                        }

                        foreach (var change in entry.changes)
                        {
                            if (change.field == "live_videos")
                            {
                                await ProcessLiveVideoEvent(change.value);
                            }
                            else if (change.field == "live_comments")
                            {
                                await ProcessLiveCommentEvent(change.value);
                            }
                            else
                            {
                                Console.WriteLine($"Evento desconhecido: {change.field}");
                            }
                        }
                    }
                }

                return Ok(); // Retorna 200 OK para a Meta
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao processar webhook: {ex.Message}");
                _ultimoErroDebug = ex.ToString(); // DEBUG TEMPORARIO
                return StatusCode(500, "Erro interno do servidor");
            }
        }

        // ENDPOINT TEMPORARIO DE DEBUG - remover junto com o debug-comentarios.
        [HttpGet("debug-ultimo-payload")]
        public IActionResult DebugUltimoPayload()
        {
            return Ok(new
            {
                recebidoEm = _ultimoRecebidoEmDebug,
                payload = _ultimoPayloadRecebidoDebug,
                erro = _ultimoErroDebug
            });
        }

        // Diagnóstico em produção usando o AccessToken configurado
        [HttpGet("debug-meta-status")]
        public async Task<IActionResult> DebugMetaStatus([FromServices] IHttpClientFactory clientFactory)
        {
            var token = _configuration["Instagram:AccessToken"];
            var igId = _configuration["Instagram:InstagramAccountId"] ?? "17841472957302808";

            if (string.IsNullOrEmpty(token))
                return Ok(new { erro = "Instagram:AccessToken está vazio no appsettings" });

            var client = clientFactory.CreateClient();
            var prefix = token.Length > 8 ? token.Substring(0, 8) + "..." : "curto";

            // 1. Validar no graph.facebook.com
            var fbMeRes = await client.GetAsync($"https://graph.facebook.com/v23.0/me?access_token={token.Trim()}");
            var fbMeContent = await fbMeRes.Content.ReadAsStringAsync();

            // 2. Validar no graph.instagram.com
            var igMeRes = await client.GetAsync($"https://graph.instagram.com/v23.0/me?fields=id,username&access_token={token.Trim()}");
            var igMeContent = await igMeRes.Content.ReadAsStringAsync();

            // 3. Subscribed apps no graph.facebook.com
            var subRes = await client.GetAsync($"https://graph.facebook.com/v23.0/{igId}/subscribed_apps?access_token={token.Trim()}");
            var subContent = await subRes.Content.ReadAsStringAsync();

            // 4. Live media no graph.instagram.com
            var igLiveRes = await client.GetAsync($"https://graph.instagram.com/v23.0/me/live_media?fields=id,status&access_token={token.Trim()}");
            var igLiveContent = await igLiveRes.Content.ReadAsStringAsync();

            return Ok(new
            {
                tokenLength = token.Length,
                tokenPrefix = prefix,
                facebookGraph_me = new { status = (int)fbMeRes.StatusCode, resp = fbMeContent },
                instagramGraph_me = new { status = (int)igMeRes.StatusCode, resp = igMeContent },
                subscribedApps = new { status = (int)subRes.StatusCode, resp = subContent },
                instagramLiveMedia = new { status = (int)igLiveRes.StatusCode, resp = igLiveContent }
            });
        }

        // Força a subscrição dos campos de live_comments e messages via Graph API
        [HttpGet("debug-forcar-inscricao")]
        public async Task<IActionResult> DebugForcarInscricao([FromServices] IHttpClientFactory clientFactory)
        {
            var token = _configuration["Instagram:AccessToken"];
            var igId = _configuration["Instagram:InstagramAccountId"] ?? "17841472957302808";

            if (string.IsNullOrEmpty(token))
                return Ok(new { erro = "Instagram:AccessToken está vazio" });

            var client = clientFactory.CreateClient();
            var res = await client.PostAsync($"https://graph.facebook.com/v23.0/{igId}/subscribed_apps?subscribed_fields=live_comments,messages,comments&access_token={token}", null);
            var content = await res.Content.ReadAsStringAsync();

            return Ok(new
            {
                statusCode = (int)res.StatusCode,
                resposta = content
            });
        }

        private async Task ProcessLiveVideoEvent(JsonElement value)
        {
            try
            {
                var liveVideoValue = JsonSerializer.Deserialize<LiveVideoValue>(value.GetRawText());
                
                if (liveVideoValue != null)
                {
                    long liveVideoId = long.Parse(liveVideoValue.id);
                    
                    // Buscar ou criar LiveSession
                    var liveSession = await _dbContext.LiveSession
                        .FirstOrDefaultAsync(l => l.LiveVideoId == liveVideoId);

                    if (liveSession == null)
                    {
                        // Criar nova LiveSession
                        liveSession = new LiveSession
                        {
                            LiveVideoId = liveVideoId,
                            Status = liveVideoValue.status,
                            StartedAt = DateTime.Now
                        };
                        _dbContext.LiveSession.Add(liveSession);
                        Console.WriteLine($"Nova live iniciada: {liveVideoId}");
                    }
                    else
                    {
                        // Atualizar LiveSession existente
                        liveSession.Status = liveVideoValue.status;
                        
                        if (liveVideoValue.status == "live_stopped")
                        {
                            liveSession.EndedAt = DateTime.UtcNow;
                            Console.WriteLine($"Live finalizada: {liveVideoId}");
                        }
                        else
                        {
                            Console.WriteLine($"Live atualizada: {liveVideoId} - Status: {liveVideoValue.status}");
                        }
                    }

                    await _dbContext.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao processar evento de live: {ex.Message}");
            }
        }

        private async Task ProcessLiveCommentEvent(JsonElement value)
        {
            try
            {
                var commentValue = JsonSerializer.Deserialize<LiveCommentValue>(value.GetRawText());
                
                if (commentValue != null)
                {
                    long liveVideoId = long.Parse(commentValue.media.id);

                    // Evita duplicidade: o InstagramLivePollingService (plano B, ja que a Meta
                    // as vezes nao entrega esse webhook) pode ter salvo esse mesmo comentario
                    // primeiro via polling.
                    if (!string.IsNullOrEmpty(commentValue.id))
                    {
                        var jaExiste = await _dbContext.ComentarioLive
                            .AnyAsync(c => c.InstagramCommentId == commentValue.id);

                        if (jaExiste)
                        {
                            Console.WriteLine($"Comentário {commentValue.id} já estava salvo (provavelmente via polling) - ignorado.");
                            return;
                        }
                    }

                    // A Meta nao possui um campo de webhook "live_videos" (nao existe na API atual),
                    // entao a LiveSession e criada aqui mesmo, no primeiro comentario recebido
                    // daquele vídeo, em vez de esperar um evento de inicio de live que nunca chega.
                    var liveSession = await _dbContext.LiveSession
                        .FirstOrDefaultAsync(l => l.LiveVideoId == liveVideoId);

                    if (liveSession == null)
                    {
                        liveSession = new LiveSession
                        {
                            LiveVideoId = liveVideoId,
                            Status = "live",
                            StartedAt = DateTime.Now
                        };
                        _dbContext.LiveSession.Add(liveSession);
                        await _dbContext.SaveChangesAsync();
                        Console.WriteLine($"Nova LiveSession criada a partir do primeiro comentário: {liveVideoId}");
                    }

                    // Criar novo comentário
                    var comentario = new ComentarioLive
                    {
                        Username = commentValue.from.username,
                        CommentText = commentValue.text,
                        CommentTimestamp = DateTime.Now,
                        CreatedAt = DateTime.Now,
                        LiveSessionId = liveVideoId,
                        InstagramCommentId = commentValue.id
                    };

                    _dbContext.ComentarioLive.Add(comentario);
                    await _dbContext.SaveChangesAsync();

                    Console.WriteLine($"Comentário salvo: {commentValue.from.username} - {commentValue.text}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao processar comentário: {ex.Message}");
                _ultimoErroDebug = ex.ToString(); // DEBUG TEMPORARIO
            }
        }
    }
}
