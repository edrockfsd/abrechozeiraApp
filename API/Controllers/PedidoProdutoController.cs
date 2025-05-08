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
    public class PedidoProdutoController : ControllerBase
    {
        private readonly AbrechozeiraContext _context;

        public PedidoProdutoController(AbrechozeiraContext context)
        {
            _context = context;
        }

        // GET: api/PedidoProduto
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PedidoProduto>>> GetPedidoProduto()
        {
            return await _context.PedidoProduto.ToListAsync();
        }

        // GET: api/PedidoProduto/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PedidoProduto>> GetPedidoProduto(int id)
        {
            var PedidoProduto = await _context.PedidoProduto.FindAsync(id);

            if (PedidoProduto == null)
            {
                return NotFound();
            }

            return PedidoProduto;
        }

        // PUT: api/PedidoProduto/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPedidoProduto(int id, PedidoProduto PedidoProduto)
        {
            if (id != PedidoProduto.Id)
            {
                return BadRequest();
            }

            _context.Entry(PedidoProduto).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PedidoProdutoExists(id))
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

        // POST: api/PedidoProduto
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<PedidoProduto>> PostPedidoProduto(PedidoProduto PedidoProduto)
        {
            _context.PedidoProduto.Add(PedidoProduto);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPedidoProduto", new { id = PedidoProduto.Id }, PedidoProduto);
        }

        // DELETE: api/PedidoProduto/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePedidoProduto(int id)
        {
            var PedidoProduto = await _context.PedidoProduto.FindAsync(id);
            if (PedidoProduto == null)
            {
                return NotFound();
            }

            _context.PedidoProduto.Remove(PedidoProduto);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PedidoProdutoExists(int id)
        {
            return _context.PedidoProduto.Any(e => e.Id == id);
        }

    }
}
