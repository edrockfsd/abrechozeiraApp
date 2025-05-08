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
    public class PedidoStatusController : ControllerBase
    {
        private readonly AbrechozeiraContext _context;

        public PedidoStatusController(AbrechozeiraContext context)
        {
            _context = context;
        }

        // GET: api/PedidoStatus
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PedidoStatus>>> GetPedidoStatus()
        {
            return await _context.PedidoStatus.ToListAsync();
        }

        // GET: api/PedidoStatus/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PedidoStatus>> GetPedidoStatus(int id)
        {
            var PedidoStatus = await _context.PedidoStatus.FindAsync(id);

            if (PedidoStatus == null)
            {
                return NotFound();
            }

            return PedidoStatus;
        }

        // PUT: api/PedidoStatus/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPedidoStatus(int id, PedidoStatus PedidoStatus)
        {
            if (id != PedidoStatus.Id)
            {
                return BadRequest();
            }

            _context.Entry(PedidoStatus).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PedidoStatusExists(id))
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

        // POST: api/PedidoStatus
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<PedidoStatus>> PostPedidoStatus(PedidoStatus PedidoStatus)
        {
            _context.PedidoStatus.Add(PedidoStatus);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPedidoStatus", new { id = PedidoStatus.Id }, PedidoStatus);
        }

        // DELETE: api/PedidoStatus/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePedidoStatus(int id)
        {
            var PedidoStatus = await _context.PedidoStatus.FindAsync(id);
            if (PedidoStatus == null)
            {
                return NotFound();
            }

            _context.PedidoStatus.Remove(PedidoStatus);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PedidoStatusExists(int id)
        {
            return _context.PedidoStatus.Any(e => e.Id == id);
        }

    }
}
