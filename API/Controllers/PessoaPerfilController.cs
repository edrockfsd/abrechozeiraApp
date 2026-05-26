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
    public class PessoaPerfilController : ControllerBase
    {
        private readonly AbrechozeiraContext _context;

        public PessoaPerfilController(AbrechozeiraContext context)
        {
            _context = context;
        }

        // GET: api/PessoaPerfil
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PessoaPerfil>>> GetPessoaPerfil()
        {
            return await _context.PessoaPerfil.ToListAsync();
        }

        // GET: api/PessoaPerfil/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PessoaPerfil>> GetPessoaPerfil(int id)
        {
            var PessoaPerfil = await _context.PessoaPerfil.FindAsync(id);

            if (PessoaPerfil == null)
            {
                return NotFound();
            }

            return PessoaPerfil;
        }

        // PUT: api/PessoaPerfil/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPessoaPerfil(int id, PessoaPerfil PessoaPerfil)
        {
            if (id != PessoaPerfil.Id)
            {
                return BadRequest();
            }

            _context.Entry(PessoaPerfil).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PessoaPerfilExists(id))
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

        // POST: api/PessoaPerfil
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<PessoaPerfil>> PostPessoaPerfil(PessoaPerfil PessoaPerfil)
        {
            _context.PessoaPerfil.Add(PessoaPerfil);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPessoaPerfil", new { id = PessoaPerfil.Id }, PessoaPerfil);
        }

        // DELETE: api/PessoaPerfil/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePessoaPerfil(int id)
        {
            var PessoaPerfil = await _context.PessoaPerfil.FindAsync(id);
            if (PessoaPerfil == null)
            {
                return NotFound();
            }

            _context.PessoaPerfil.Remove(PessoaPerfil);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PessoaPerfilExists(int id)
        {
            return _context.PessoaPerfil.Any(e => e.Id == id);
        }
    }
}
