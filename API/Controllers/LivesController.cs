using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ABrechozeiraApp.Models;
using ABrechozeiraApp.Services;
using Microsoft.OpenApi.Extensions;
using System.Globalization;

namespace ABrechozeiraApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LivesController : ControllerBase
    {
        private readonly AbrechozeiraContext _context;
        private readonly WhatsAppService _whatsApp;
        private readonly ILogger<LivesController> _logger;

        public LivesController(AbrechozeiraContext context, WhatsAppService whatsApp, ILogger<LivesController> logger)
        {
            _context = context;
            _whatsApp = whatsApp;
            _logger = logger;
        }

        // ─── Aviso de live por WhatsApp (template aviso_live_v1) ─────────────────

        public class AvisoLiveEnvioInput
        {
            public int PessoaId { get; set; }
            public string LinkLive { get; set; } = "";
        }

        /// <summary>
        /// Clientes que recebem o aviso de live: categoria "Cliente", ativas e com telefone.
        /// Um envio por número de telefone (cadastros duplicados com o mesmo número recebem uma vez só).
        /// </summary>
        [Authorize]
        [HttpGet("AvisoLive/Destinatarios")]
        public async Task<IActionResult> GetDestinatariosAvisoLive()
        {
            var clientes = await _context.Pessoa
                .Where(p => p.StatusId == 1
                         && p.PessoaCategoria!.Descricao == "Cliente"
                         && p.Telefone != null && p.Telefone != "")
                .OrderBy(p => p.Nome)
                .Select(p => new { p.Id, p.Nome, p.NickName, p.Telefone })
                .ToListAsync();

            var destinatarios = clientes
                .Select(c => new { c.Id, Nome = PrimeiroNome(c.Nome, c.NickName), c.Telefone, Digitos = new string(c.Telefone!.Where(char.IsDigit).ToArray()) })
                .Where(c => c.Digitos.Length >= 10)
                .GroupBy(c => c.Digitos.Length > 11 ? c.Digitos[^11..] : c.Digitos)
                .Select(g => g.First())
                .Select(c => new { id = c.Id, nome = c.Nome, telefone = c.Telefone })
                .ToList();

            return Ok(destinatarios);
        }

        /// <summary>Envia o aviso de live para uma cliente (o frontend chama uma vez por cliente, mostrando o progresso).</summary>
        [Authorize]
        [HttpPost("AvisoLive/Enviar")]
        public async Task<IActionResult> EnviarAvisoLive([FromBody] AvisoLiveEnvioInput input)
        {
            var caminho = ExtrairCaminhoInstagram(input.LinkLive);
            if (caminho == null)
                return BadRequest(new { message = "Informe o link da live do Instagram (ex.: https://www.instagram.com/abrechozeira/live/...)." });

            var pessoa = await _context.Pessoa.Where(p => p.Id == input.PessoaId)
                .Select(p => new { p.Nome, p.NickName, p.Telefone }).FirstOrDefaultAsync();
            if (pessoa == null) return NotFound(new { message = "Cliente não encontrada." });
            if (string.IsNullOrWhiteSpace(pessoa.Telefone)) return BadRequest(new { message = "Cliente sem telefone cadastrado." });

            var enviado = await _whatsApp.EnviarAvisoLiveAsync(pessoa.Telefone, PrimeiroNome(pessoa.Nome, pessoa.NickName), caminho);
            if (!enviado)
                _logger.LogWarning("Aviso de live não enviado para a pessoa {PessoaId}.", input.PessoaId);

            return Ok(new { enviado, erro = enviado ? null : "WhatsApp recusou o envio (verifique o telefone)" });
        }

        /// <summary>"https://www.instagram.com/abrechozeira/live/123?igsh=x" → "abrechozeira/live/123?igsh=x". Só aceita links do Instagram.</summary>
        private static string? ExtrairCaminhoInstagram(string? link)
        {
            if (string.IsNullOrWhiteSpace(link)) return null;
            var texto = link.Trim();
            if (!texto.StartsWith("http", StringComparison.OrdinalIgnoreCase)) texto = "https://" + texto;
            if (!Uri.TryCreate(texto, UriKind.Absolute, out var uri)) return null;
            if (!uri.Host.Equals("instagram.com", StringComparison.OrdinalIgnoreCase)
                && !uri.Host.EndsWith(".instagram.com", StringComparison.OrdinalIgnoreCase)) return null;

            var caminho = (uri.PathAndQuery + uri.Fragment).TrimStart('/');
            return string.IsNullOrWhiteSpace(caminho) ? null : caminho;
        }

        private static string PrimeiroNome(string? nome, string? nickName)
        {
            var primeiro = (nome ?? "").Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(primeiro) && !primeiro.StartsWith("@")) return primeiro;
            var nick = (nickName ?? primeiro ?? "").TrimStart('@').Trim();
            return string.IsNullOrWhiteSpace(nick) ? "tudo bem" : nick;
        }

        // GET: api/Lives
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Live>>> GetLive()
        {
            return await _context.Live.OrderByDescending(l => l.DataLive).ToListAsync();
        }

        // GET: api/Lives/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Live>> GetLive(int id)
        {
            var live = await _context.Live.FindAsync(id);

            if (live == null)
            {
                return NotFound();
            }

            return live;
        }

        // PUT: api/Lives/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLive(int id, Live live)
        {
            if (id != live.Id)
            {
                return BadRequest();
            }

            _context.Entry(live).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LiveExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/Lives
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Live>> PostLive(Live live)
        {
            _context.Live.Add(live);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetLive", new { id = live.Id }, live);
        }

        // DELETE: api/Lives/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLive(int id)
        {
            var live = await _context.Live.FindAsync(id);
            if (live == null)
            {
                return NotFound();
            }

            _context.Live.Remove(live);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool LiveExists(int id)
        {
            return _context.Live.Any(e => e.Id == id);
        }

        // PATCH: api/Lives/5/toggle-status
        [HttpPatch("{id}/toggle-status")]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            var live = await _context.Live.FindAsync(id);
            if (live == null)
            {
                return NotFound();
            }

            live.Ativo = !live.Ativo;
            live.DataAlteracao = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new { id = live.Id, ativo = live.Ativo });
        }

        [HttpGet("GetLiveCompleta")]
        public IActionResult GetLiveCompleta()
        {
            var lives = from liv in _context.Live
                        select new
                        {
                            liv.Id,
                            liv.Titulo,
                            liv.Observacoes,
                            liv.DataLive,
                            liv.DataAlteracao,
                            liv.Ativo,
                            DiaSemana = DateTimeFormatInfo.CurrentInfo.GetDayName(liv.DataLive.DayOfWeek)
                        };

            return Ok(lives.ToList());
        }

        /// <summary>
        /// Retorna os dados de ID e Descrição da live
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetLivesCombo")]
        public IActionResult GetLivesCombo([FromQuery] bool apenasAtivos = false)
        {
            var query = _context.Live.AsQueryable();
            if (apenasAtivos)
            {
                query = query.Where(liv => liv.Ativo);
            }

            var lives = from liv in query
                        orderby liv.DataLive descending
                        select new
                        {
                            liv.Id,
                            liv.Titulo
                        };

            return Ok(lives.ToList());
        }
    }
}
