using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ABrechozeiraApp.Models;

namespace ABrechozeiraApp.Controllers
{
    // DTOs de leitura. Mantidos aqui (em vez de em Models) porque são apenas
    // projeções de API, sem correspondência 1:1 com uma tabela.
    public class LiveSessionResumoDto
    {
        public int Id { get; set; }
        public long LiveVideoId { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime StartedAt { get; set; }
        public DateTime? EndedAt { get; set; }
        public int TotalComentarios { get; set; }
        public DateTime? PrimeiroComentarioEm { get; set; }
        public DateTime? UltimoComentarioEm { get; set; }
    }

    public class ComentarioLiveDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string CommentText { get; set; } = string.Empty;
        public DateTime CommentTimestamp { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class LiveSessionsController : ControllerBase
    {
        private readonly AbrechozeiraContext _context;

        public LiveSessionsController(AbrechozeiraContext context)
        {
            _context = context;
        }

        // GET: api/LiveSessions
        // Lista as sessões de live (transmissões do Instagram) com contagem de comentários.
        //
        // Observação importante sobre o relacionamento:
        // ComentarioLive.LiveSessionId NÃO é a chave estrangeira para LiveSession.Id.
        // Ele guarda o ID bruto da mídia/live do Instagram (o mesmo valor de
        // LiveSession.LiveVideoId), porque é assim que o InstagramWebhookController
        // grava os comentários (só tem o media.id do payload da Meta, não o Id
        // interno). Por isso o JOIN abaixo usa LiveVideoId == ComentarioLive.LiveSessionId.
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LiveSessionResumoDto>>> GetLiveSessions()
        {
            var sessions = await (
                from ls in _context.LiveSession
                join c in _context.ComentarioLive
                    on ls.LiveVideoId equals c.LiveSessionId into comentarios
                orderby ls.StartedAt descending
                select new LiveSessionResumoDto
                {
                    Id = ls.Id,
                    LiveVideoId = ls.LiveVideoId,
                    Status = ls.Status,
                    StartedAt = ls.StartedAt,
                    EndedAt = ls.EndedAt,
                    TotalComentarios = comentarios.Count(),
                    PrimeiroComentarioEm = comentarios.Min(c => (DateTime?)c.CommentTimestamp),
                    UltimoComentarioEm = comentarios.Max(c => (DateTime?)c.CommentTimestamp)
                }
            ).ToListAsync();

            return Ok(sessions);
        }

        // GET: api/LiveSessions/5
        [HttpGet("{id}")]
        public async Task<ActionResult<LiveSessionResumoDto>> GetLiveSession(int id)
        {
            var liveSession = await _context.LiveSession.FindAsync(id);
            if (liveSession == null)
            {
                return NotFound();
            }

            var comentariosQuery = _context.ComentarioLive
                .Where(c => c.LiveSessionId == liveSession.LiveVideoId);

            var dto = new LiveSessionResumoDto
            {
                Id = liveSession.Id,
                LiveVideoId = liveSession.LiveVideoId,
                Status = liveSession.Status,
                StartedAt = liveSession.StartedAt,
                EndedAt = liveSession.EndedAt,
                TotalComentarios = await comentariosQuery.CountAsync(),
                PrimeiroComentarioEm = await comentariosQuery.MinAsync(c => (DateTime?)c.CommentTimestamp),
                UltimoComentarioEm = await comentariosQuery.MaxAsync(c => (DateTime?)c.CommentTimestamp)
            };

            return Ok(dto);
        }

        // GET: api/LiveSessions/5/comentarios
        [HttpGet("{id}/comentarios")]
        public async Task<ActionResult<IEnumerable<ComentarioLiveDto>>> GetComentarios(int id)
        {
            var liveSession = await _context.LiveSession.FindAsync(id);
            if (liveSession == null)
            {
                return NotFound();
            }

            var comentarios = await _context.ComentarioLive
                .Where(c => c.LiveSessionId == liveSession.LiveVideoId)
                .OrderBy(c => c.CommentTimestamp)
                .Select(c => new ComentarioLiveDto
                {
                    Id = c.Id,
                    Username = c.Username,
                    CommentText = c.CommentText,
                    CommentTimestamp = c.CommentTimestamp,
                    CreatedAt = c.CreatedAt
                })
                .ToListAsync();

            return Ok(comentarios);
        }
    }
}
