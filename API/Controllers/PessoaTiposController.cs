using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ABrechozeiraApp.Models;

namespace ABrechozeiraApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PessoaTiposController : ControllerBase
    {
        private readonly AbrechozeiraContext _context;

        public PessoaTiposController(AbrechozeiraContext context)
        {
            _context = context;
        }

        // GET: api/PessoaTipos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PessoaTipo>>> GetPessoaTipos()
        {
            return await _context.PessoaTipo.ToListAsync();
        }

        // GET: api/PessoaTipos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PessoaTipo>> GetPessoaTipo(int id)
        {
            var pessoaTipo = await _context.PessoaTipo.FindAsync(id);

            if (pessoaTipo == null)
            {
                return NotFound();
            }

            return pessoaTipo;
        }

        // PUT: api/PessoaTipos/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPessoaTipo(int id, PessoaTipo pessoaTipo)
        {
            if (id != pessoaTipo.Id)
            {
                return BadRequest();
            }

            _context.Entry(pessoaTipo).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PessoaTipoExists(id))
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

        // POST: api/PessoaTipos
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<PessoaTipo>> PostPessoaTipo(PessoaTipo pessoaTipo)
        {
            _context.PessoaTipo.Add(pessoaTipo);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPessoaTipo", new { id = pessoaTipo.Id }, pessoaTipo);
        }

        // DELETE: api/PessoaTipos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePessoaTipo(int id)
        {
            var pessoaTipo = await _context.PessoaTipo.FindAsync(id);
            if (pessoaTipo == null)
            {
                return NotFound();
            }

            _context.PessoaTipo.Remove(pessoaTipo);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PessoaTipoExists(int id)
        {
            return _context.PessoaTipo.Any(e => e.Id == id);
        }
    }
}
