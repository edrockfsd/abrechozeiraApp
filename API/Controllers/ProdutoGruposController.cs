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
    public class ProdutoGruposController : ControllerBase
    {
        private readonly AbrechozeiraContext _context;

        public ProdutoGruposController(AbrechozeiraContext context)
        {
            _context = context;
        }

        
        // GET: api/Grupos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProdutoGrupo>>> GetGrupo()
        {
            return await _context.ProdutoGrupo.ToListAsync();
        }

        // GET: api/Grupos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ProdutoGrupo>> GetGrupo(int id)
        {
            var grupo = await _context.ProdutoGrupo.FindAsync(id);

            if (grupo == null)
            {
                return NotFound();
            }

            return grupo;
        }

        // PUT: api/Grupos/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProdutoGrupo(int id, ProdutoGrupo grupo)
        {
            if (id != grupo.Id)
            {
                return BadRequest();
            }

            _context.Entry(grupo).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProdutoGrupoExists(id))
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

        // POST: api/Grupos
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<ProdutoGrupo>> ProdutoGrupo(ProdutoGrupo grupo)
        {
            _context.ProdutoGrupo.Add(grupo);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetProdutoGrupo", new { id = grupo.Id }, grupo);
        }

        // DELETE: api/ProdutoGrupos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProdutoGrupo(int id)
        {
            var grupo = await _context.ProdutoGrupo.FindAsync(id);
            if (grupo == null)
            {
                return NotFound();
            }

            _context.ProdutoGrupo.Remove(grupo);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProdutoGrupoExists(int id)
        {
            return _context.ProdutoGrupo.Any(e => e.Id == id);
        }
    }
}
