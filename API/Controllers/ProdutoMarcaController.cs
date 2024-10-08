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
    public class ProdutoMarcaController : ControllerBase
    {
        private readonly AbrechozeiraContext _context;

        public ProdutoMarcaController(AbrechozeiraContext context)
        {
            _context = context;
        }

        // GET: api/ProdutoMarca
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProdutoMarca>>> GetProdutoMarca()
        {
            return await _context.ProdutoMarca.ToListAsync();
        }

        // GET: api/ProdutoMarca/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ProdutoMarca>> GetProdutoMarca(int id)
        {
            var ProdutoMarca = await _context.ProdutoMarca.FindAsync(id);

            if (ProdutoMarca == null)
            {
                return NotFound();
            }

            return ProdutoMarca;
        }

        // PUT: api/ProdutoMarca/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProdutoMarca(int id, ProdutoMarca ProdutoMarca)
        {
            if (id != ProdutoMarca.Id)
            {
                return BadRequest();
            }

            _context.Entry(ProdutoMarca).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProdutoMarcaExists(id))
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

        // POST: api/ProdutoMarca
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<ProdutoMarca>> PostProdutoMarca(ProdutoMarca ProdutoMarca)
        {
            _context.ProdutoMarca.Add(ProdutoMarca);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetProdutoMarca", new { id = ProdutoMarca.Id }, ProdutoMarca);
        }

        // DELETE: api/ProdutoMarca/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProdutoMarca(int id)
        {
            var ProdutoMarca = await _context.ProdutoMarca.FindAsync(id);
            if (ProdutoMarca == null)
            {
                return NotFound();
            }

            _context.ProdutoMarca.Remove(ProdutoMarca);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProdutoMarcaExists(int id)
        {
            return _context.ProdutoMarca.Any(e => e.Id == id);
        }
    }
}
