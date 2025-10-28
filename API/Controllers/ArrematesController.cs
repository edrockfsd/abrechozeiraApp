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
    public class ArrematesController : ControllerBase
    {
        private readonly AbrechozeiraContext _context;

        public ArrematesController(AbrechozeiraContext context)
        {
            _context = context;
        }

        // GET: api/ArrematesArremate
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Arremate>>> GetArremate()
        {
            return await _context.Arremate.ToListAsync();
        }

        // GET: api/Arremates/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Arremate>> GetArremate(int id)
        {
            var arremate = await _context.Arremate.FindAsync(id);

            if (arremate == null)
            {
                return NotFound();
            }

            return arremate;
        }

        // PUT: api/Arremates/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutArremate(int id, Arremate arremate)
        {
            if (id != arremate.Id)
            {
                return BadRequest();
            }

            _context.Entry(arremate).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ArremateExists(id))
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

        // POST: api/Arremates
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Arremate>> PostArremate(Arremate arremate)
        {
            _context.Arremate.Add(arremate);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetArremate", new { id = arremate.Id }, arremate);
        }

        // DELETE: api/Arremates/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteArremate(int id)
        {
            var arremate = await _context.Arremate.FindAsync(id);
            if (arremate == null)
            {
                return NotFound();
            }

            _context.Arremate.Remove(arremate);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ArremateExists(int id)
        {
            return _context.Arremate.Any(e => e.Id == id);
        }

        [HttpGet("GetArremateCompleto")]
        public IActionResult GetArremateCompleto()
        {
            var arremates = from arr in _context.Arremate
                        join prod in _context.Produto on arr.ProdutoId equals prod.Id
                        join liv in _context.Live on arr.ProdutoId equals liv.Id
                        select new
                        {
                            arr.Id,
                            liveId = liv.Id,
                            produtoID = prod.Id,
                            arr.Arrematante,
                            arr.Observacoes,
                            arr.DataArremate,
                            arr.DataAlteracao,
                            UsuarioModificacaoId = 1 //TODO TESTE/IMPLEMENTAR
                        };



            return Ok(arremates.ToList());
        }

        [HttpGet("GetArrematesByLiveID")]
        public IActionResult GetArrematesByLiveID(int liveID)
        {
            var arremates = from arr in _context.Arremate
                            join prod in _context.Produto on arr.ProdutoId equals prod.Id
                            join est in _context.Estoque on prod.Id equals est.ProdutoId
                            where arr.LiveId == liveID
                            select new
                            {
                                arr.Id,
                                produtoID = prod.Id,
                                produtoDescricao = prod.Descricao,
                                arr.Arrematante,
                                arr.Observacoes,
                                arr.DataArremate,
                                arr.CodigoLive,
                                est.CodigoEstoque,
                                arr.ValorArremate
                            };



            return Ok(arremates.ToList());
        }
    }
}
