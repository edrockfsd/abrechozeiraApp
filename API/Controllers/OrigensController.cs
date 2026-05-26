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
    public class OrigensController : ControllerBase
    {
        private readonly AbrechozeiraContext _context;

        public OrigensController(AbrechozeiraContext context)
        {
            _context = context;
        }

        // GET: api/Origens
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Origem>>> GetOrigem()
        {
            return await _context.Origem.ToListAsync();
        }

        // GET: api/Origens/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Origem>> GetOrigem(int id)
        {
            var origem = await _context.Origem.FindAsync(id);

            if (origem == null)
            {
                return NotFound();
            }

            return origem;
        }

        // PUT: api/Origens/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutOrigem(int id, Origem origem)
        {
            if (id != origem.Id)
            {
                return BadRequest();
            }

            _context.Entry(origem).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!OrigemExists(id))
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

        // POST: api/Origens
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Origem>> PostOrigem(Origem origem)
        {
            _context.Origem.Add(origem);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetOrigem", new { id = origem.Id }, origem);
        }

        // DELETE: api/Origens/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteOrigem(int id)
        {
            var origem = await _context.Origem.FindAsync(id);
            if (origem == null)
            {
                return NotFound();
            }

            _context.Origem.Remove(origem);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool OrigemExists(int id)
        {
            return _context.Origem.Any(e => e.Id == id);
        }
    }
}
