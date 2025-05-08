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
    public class FormaPagamentoController : ControllerBase
    {
        private readonly AbrechozeiraContext _context;

        public FormaPagamentoController(AbrechozeiraContext context)
        {
            _context = context;
        }

        // GET: api/FormaPagamento
        [HttpGet]
        public async Task<ActionResult<IEnumerable<FormaPagamento>>> GetFormaPagamento()
        {
            return await _context.FormaPagamento.ToListAsync();
        }

        // GET: api/FormaPagamento/5
        [HttpGet("{id}")]
        public async Task<ActionResult<FormaPagamento>> GetFormaPagamento(int id)
        {
            var FormaPagamento = await _context.FormaPagamento.FindAsync(id);

            if (FormaPagamento == null)
            {
                return NotFound();
            }

            return FormaPagamento;
        }

        // PUT: api/FormaPagamento/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutFormaPagamento(int id, FormaPagamento FormaPagamento)
        {
            if (id != FormaPagamento.Id)
            {
                return BadRequest();
            }

            _context.Entry(FormaPagamento).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!FormaPagamentoExists(id))
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

        // POST: api/FormaPagamento
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<FormaPagamento>> PostFormaPagamento(FormaPagamento FormaPagamento)
        {
            _context.FormaPagamento.Add(FormaPagamento);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetFormaPagamento", new { id = FormaPagamento.Id }, FormaPagamento);
        }

        // DELETE: api/FormaPagamento/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteFormaPagamento(int id)
        {
            var FormaPagamento = await _context.FormaPagamento.FindAsync(id);
            if (FormaPagamento == null)
            {
                return NotFound();
            }

            _context.FormaPagamento.Remove(FormaPagamento);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool FormaPagamentoExists(int id)
        {
            return _context.FormaPagamento.Any(e => e.Id == id);
        }

    }
}
