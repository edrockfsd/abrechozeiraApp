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

                var root = JsonSerializer.Deserialize<InstagramWebhookRoot>(body);

                if (root?.entry != null)
                {
                    foreach (var entry in root.entry)
                    {
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
                return StatusCode(500, "Erro interno do servidor");
            }
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
                    
                    // Criar novo comentário
                    var comentario = new ComentarioLive
                    {
                        Username = commentValue.from.username,
                        CommentText = commentValue.text,
                        CommentTimestamp = DateTime.Now,
                        CreatedAt = DateTime.Now,
                        LiveSessionId = liveVideoId
                    };

                    _dbContext.ComentarioLive.Add(comentario);
                    await _dbContext.SaveChangesAsync();

                    Console.WriteLine($"Comentário salvo: {commentValue.from.username} - {commentValue.text}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao processar comentário: {ex.Message}");
            }
        }
    }
}
