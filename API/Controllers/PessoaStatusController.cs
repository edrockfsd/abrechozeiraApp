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
    public class PessoaStatusController : ControllerBase
    {
        private readonly AbrechozeiraContext _context;

        public PessoaStatusController(AbrechozeiraContext context)
        {
            _context = context;
        }

        // GET: api/PessoaStatus
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PessoaStatus>>> GetPessoaStatus()
        {
            return await _context.PessoaStatus.ToListAsync();
        }

        // GET: api/PessoaStatus/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PessoaStatus>> GetPessoaStatus(int id)
        {
            var PessoaStatus = await _context.PessoaStatus.FindAsync(id);

            if (PessoaStatus == null)
            {
                return NotFound();
            }

            return PessoaStatus;
        }

        // PUT: api/PessoaStatus/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPessoaStatus(int id, PessoaStatus PessoaStatus)
        {
            if (id != PessoaStatus.Id)
            {
                return BadRequest();
            }

            _context.Entry(PessoaStatus).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PessoaStatusExists(id))
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

        // POST: api/PessoaStatus
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<PessoaStatus>> PostPessoaStatus(PessoaStatus PessoaStatus)
        {
            _context.PessoaStatus.Add(PessoaStatus);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPessoaStatus", new { id = PessoaStatus.Id }, PessoaStatus);
        }

        // DELETE: api/PessoaStatus/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePessoaStatus(int id)
        {
            var PessoaStatus = await _context.PessoaStatus.FindAsync(id);
            if (PessoaStatus == null)
            {
                return NotFound();
            }

            _context.PessoaStatus.Remove(PessoaStatus);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PessoaStatusExists(int id)
        {
            return _context.PessoaStatus.Any(e => e.Id == id);
        }
    }
}
