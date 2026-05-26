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
    public class TipoEnderecoController : ControllerBase
    {
        private readonly AbrechozeiraContext _context;

        public TipoEnderecoController(AbrechozeiraContext context)
        {
            _context = context;
        }

        // GET: api/TipoEndereco
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TipoEndereco>>> GetTipoEndereco()
        {
            return await _context.TipoEndereco.ToListAsync();
        }

        // GET: api/TipoEndereco/5
        [HttpGet("{id}")]
        public async Task<ActionResult<TipoEndereco>> GetTipoEndereco(int id)
        {
            var TipoEndereco = await _context.TipoEndereco.FindAsync(id);

            if (TipoEndereco == null)
            {
                return NotFound();
            }

            return TipoEndereco;
        }

        // PUT: api/TipoEndereco/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutTipoEndereco(int id, TipoEndereco TipoEndereco)
        {
            if (id != TipoEndereco.Id)
            {
                return BadRequest();
            }

            _context.Entry(TipoEndereco).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TipoEnderecoExists(id))
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

        // POST: api/TipoEndereco
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<TipoEndereco>> PostTipoEndereco(TipoEndereco TipoEndereco)
        {
            _context.TipoEndereco.Add(TipoEndereco);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetTipoEndereco", new { id = TipoEndereco.Id }, TipoEndereco);
        }

        // DELETE: api/TipoEndereco/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTipoEndereco(int id)
        {
            var TipoEndereco = await _context.TipoEndereco.FindAsync(id);
            if (TipoEndereco == null)
            {
                return NotFound();
            }

            _context.TipoEndereco.Remove(TipoEndereco);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TipoEnderecoExists(int id)
        {
            return _context.TipoEndereco.Any(e => e.Id == id);
        }

    }
}
