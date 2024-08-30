using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ABrechozeiraApp.Models;
using Microsoft.OpenApi.Extensions;
using System.Globalization;

namespace ABrechozeiraApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LivesController : ControllerBase
    {
        private readonly AbrechozeiraContext _context;

        public LivesController(AbrechozeiraContext context)
        {
            _context = context;
        }

        // GET: api/Lives
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Live>>> GetLive()
        {
            return await _context.Live.ToListAsync();
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
                            DiaSemana = DateTimeFormatInfo.CurrentInfo.GetDayName(liv.DataLive.DayOfWeek)
                        };



            return Ok(lives.ToList());
        }

        /// <summary>
        /// Retorna os dados de ID e Descrição da live
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetLivesCombo")]
        public IActionResult GetLivesCombo()
        {
            var lives = from liv in _context.Live
                        select new
                        {
                            liv.Id,
                            liv.Titulo
                        };



            return Ok(lives.ToList());
        }
    }
}
