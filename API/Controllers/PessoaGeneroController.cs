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
    public class PessoaGeneroController : ControllerBase
    {
        private readonly AbrechozeiraContext _context;

        public PessoaGeneroController(AbrechozeiraContext context)
        {
            _context = context;
        }

        // GET: api/PessoaGenero
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PessoaGenero>>> GetPessoaGenero()
        {
            return await _context.PessoaGenero.ToListAsync();
        }

        // GET: api/PessoaGenero/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PessoaGenero>> GetPessoaGenero(int id)
        {
            var PessoaGenero = await _context.PessoaGenero.FindAsync(id);

            if (PessoaGenero == null)
            {
                return NotFound();
            }

            return PessoaGenero;
        }

        // PUT: api/PessoaGenero/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPessoaGenero(int id, PessoaGenero PessoaGenero)
        {
            if (id != PessoaGenero.Id)
            {
                return BadRequest();
            }

            _context.Entry(PessoaGenero).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PessoaGeneroExists(id))
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

        // POST: api/PessoaGenero
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<PessoaGenero>> PostPessoaGenero(PessoaGenero PessoaGenero)
        {
            _context.PessoaGenero.Add(PessoaGenero);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPessoaGenero", new { id = PessoaGenero.Id }, PessoaGenero);
        }

        // DELETE: api/PessoaGenero/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePessoaGenero(int id)
        {
            var PessoaGenero = await _context.PessoaGenero.FindAsync(id);
            if (PessoaGenero == null)
            {
                return NotFound();
            }

            _context.PessoaGenero.Remove(PessoaGenero);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PessoaGeneroExists(int id)
        {
            return _context.PessoaGenero.Any(e => e.Id == id);
        }
    }
}
