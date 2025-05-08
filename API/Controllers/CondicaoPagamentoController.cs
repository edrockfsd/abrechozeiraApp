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
    public class CondicaoPagamentoController : ControllerBase
    {
        private readonly AbrechozeiraContext _context;

        public CondicaoPagamentoController(AbrechozeiraContext context)
        {
            _context = context;
        }

        // GET: api/CondicaoPagamento
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CondicaoPagamento>>> GetCondicaoPagamento()
        {
            return await _context.CondicaoPagamento.ToListAsync();
        }

        // GET: api/CondicaoPagamento/5
        [HttpGet("{id}")]
        public async Task<ActionResult<CondicaoPagamento>> GetCondicaoPagamento(int id)
        {
            var CondicaoPagamento = await _context.CondicaoPagamento.FindAsync(id);

            if (CondicaoPagamento == null)
            {
                return NotFound();
            }

            return CondicaoPagamento;
        }

        // PUT: api/CondicaoPagamento/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCondicaoPagamento(int id, CondicaoPagamento CondicaoPagamento)
        {
            if (id != CondicaoPagamento.Id)
            {
                return BadRequest();
            }

            _context.Entry(CondicaoPagamento).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CondicaoPagamentoExists(id))
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

        // POST: api/CondicaoPagamento
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<CondicaoPagamento>> PostCondicaoPagamento(CondicaoPagamento CondicaoPagamento)
        {
            _context.CondicaoPagamento.Add(CondicaoPagamento);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCondicaoPagamento", new { id = CondicaoPagamento.Id }, CondicaoPagamento);
        }

        // DELETE: api/CondicaoPagamento/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCondicaoPagamento(int id)
        {
            var CondicaoPagamento = await _context.CondicaoPagamento.FindAsync(id);
            if (CondicaoPagamento == null)
            {
                return NotFound();
            }

            _context.CondicaoPagamento.Remove(CondicaoPagamento);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CondicaoPagamentoExists(int id)
        {
            return _context.CondicaoPagamento.Any(e => e.Id == id);
        }

    }
}
