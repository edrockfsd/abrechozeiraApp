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
    public class PessoaCategoriasController : ControllerBase
    {
        private readonly AbrechozeiraContext _context;

        public PessoaCategoriasController(AbrechozeiraContext context)
        {
            _context = context;
        }

        // GET: api/PessoaCategorias
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PessoaCategoria>>> GetPessoaCategoria()
        {
            return await _context.PessoaCategoria.ToListAsync();
        }

        // GET: api/PessoaCategorias/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PessoaCategoria>> GetPessoaCategoria(int id)
        {
            var pessoaCategoria = await _context.PessoaCategoria.FindAsync(id);

            if (pessoaCategoria == null)
            {
                return NotFound();
            }

            return pessoaCategoria;
        }

        // PUT: api/PessoaCategorias/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPessoaCategoria(int id, PessoaCategoria pessoaCategoria)
        {
            if (id != pessoaCategoria.Id)
            {
                return BadRequest();
            }

            _context.Entry(pessoaCategoria).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PessoaCategoriaExists(id))
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

        // POST: api/PessoaCategorias
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<PessoaCategoria>> PostPessoaCategoria(PessoaCategoria pessoaCategoria)
        {
            _context.PessoaCategoria.Add(pessoaCategoria);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPessoaCategoria", new { id = pessoaCategoria.Id }, pessoaCategoria);
        }

        // DELETE: api/PessoaCategorias/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePessoaCategoria(int id)
        {
            var pessoaCategoria = await _context.PessoaCategoria.FindAsync(id);
            if (pessoaCategoria == null)
            {
                return NotFound();
            }

            _context.PessoaCategoria.Remove(pessoaCategoria);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PessoaCategoriaExists(int id)
        {
            return _context.PessoaCategoria.Any(e => e.Id == id);
        }
    }
}
