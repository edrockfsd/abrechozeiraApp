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
    public class EnderecoController : ControllerBase
    {
        private readonly AbrechozeiraContext _context;

        public EnderecoController(AbrechozeiraContext context)
        {
            _context = context;
        }

        // GET: api/Endereco
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Endereco>>> GetEndereco()
        {
            return await _context.Endereco.ToListAsync();
        }

        // GET: api/Endereco/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Endereco>> GetEndereco(int id)
        {
            var Endereco = await _context.Endereco.FindAsync(id);

            if (Endereco == null)
            {
                return NotFound();
            }

            return Endereco;
        }

        // PUT: api/Endereco/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEndereco(int id, Endereco Endereco)
        {
            if (id != Endereco.Id)
            {
                return BadRequest();
            }

            _context.Entry(Endereco).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EnderecoExists(id))
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

        // POST: api/Endereco
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Endereco>> PostEndereco(Endereco Endereco)
        {
            _context.Endereco.Add(Endereco);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetEndereco", new { id = Endereco.Id }, Endereco);
        }

        // DELETE: api/Endereco/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEndereco(int id)
        {
            var Endereco = await _context.Endereco.FindAsync(id);
            if (Endereco == null)
            {
                return NotFound();
            }

            _context.Endereco.Remove(Endereco);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool EnderecoExists(int id)
        {
            return _context.Endereco.Any(e => e.Id == id);
        }

        [HttpGet("GetEnderecoPorPessoa")]
        public IActionResult GetEnderecoPorPessoa(int pessoaID)
        {
            var enderecos = from end in _context.Endereco
                            join tipoEndereco in _context.TipoEndereco on end.TipoEnderecoId equals tipoEndereco.Id
                            where end.PessoaID == pessoaID
                            select new
                            {
                                end.Id,
                                tipoEndereco = tipoEndereco.Descricao,
                                end.CEP,
                                end.Logradouro,
                                end.Unidade,
                                end.Complemento,
                                end.Bairro,
                                end.Localidade,
                                end.Estado
                            };
                            



            return Ok(enderecos.ToList());
        }

    }
}
