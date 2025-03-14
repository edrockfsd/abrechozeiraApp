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
    public class EstoqueController : ControllerBase
    {
        private readonly AbrechozeiraContext _context;

        public EstoqueController(AbrechozeiraContext context)
        {
            _context = context;
        }

        // GET: api/Estoques
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Estoque>>> GetEstoques()
        {
            return await _context.Estoque.ToListAsync();
        }

        // GET: api/Estoques/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Estoque>> GetEstoque(int id)
        {
            var Estoque = await _context.Estoque.FindAsync(id);

            if (Estoque == null)
            {
                return new Estoque() { Id = 0 };
            }

            return Estoque;
        }

        // PUT: api/Estoques/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEstoque(int id, Estoque Estoque)
        {
            if (id != Estoque.Id)
            {
                return BadRequest();
            }

            _context.Entry(Estoque).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!EstoqueExists(id))
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

        // POST: api/Estoques
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Estoque>> PostEstoque(Estoque Estoque)
        {
            _context.Estoque.Add(Estoque);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetEstoque", new { id = Estoque.Id }, Estoque);
        }

        // DELETE: api/Estoques/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEstoque(int id)
        {
            var Estoque = await _context.Estoque.FindAsync(id);
            if (Estoque == null)
            {
                return NotFound();
            }

            _context.Estoque.Remove(Estoque);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        [HttpGet]
        private bool EstoqueExists(int id)
        {
            return _context.Estoque.Any(e => e.Id == id);
        }

        [HttpGet("GetEstoquesCompleto")]
        public IActionResult GetEstoquesCompleto()
        {
            var estoques = from prd in _context.Produto
                           join est in _context.Estoque on prd.Id equals est.ProdutoId into estGroup
                           from est in estGroup.DefaultIfEmpty()
                           join pes in _context.Pessoa on prd.UsuarioModificacaoId equals pes.Id into pesGroup
                           from pes in pesGroup.DefaultIfEmpty()
                           select new
                           {
                               Id = est != null ? est.Id : (int?)null, // Handle null for est.Id
                               Quantidade = est != null ? est.Quantidade : (int?)null, // Handle null for est.Quantidade
                               Localizacao = est != null ? est.Localizacao : null, // Handle null for est.Localizacao
                               ProdutoId = est != null ? est.ProdutoId : (int?)null, // Handle null for est.ProdutoId
                               CodigoEstoque = est != null ? est.CodigoEstoque : null, // Handle null for est.CodigoEstoque
                               Descricao = prd.Descricao, // prd is not null because it's the main table
                               DataAlteracao = est != null ? est.DataAlteracao : (DateTime?)null, // Handle null for est.DataAlteracao
                               Nome = pes != null ? pes.Nome : null // Handle null for pes.Nome
                           };

            return Ok(estoques.ToList());
        }
        [HttpGet("GetEstoqueByCodigoEstoque")]
        public IActionResult GetEstoqueByCodigoEstoque(int codigoEstoque)
        {
            var estoques = from prd in _context.Produto
                           join est in _context.Estoque on prd.Id equals est.ProdutoId
                           join pes in _context.Pessoa on est.UsuarioModificacaoId equals pes.Id
                           where est.CodigoEstoque == codigoEstoque
                           select new
                           {
                               est.Id,
                               est.Quantidade,
                               est.Localizacao,
                               est.ProdutoId,
                               est.CodigoEstoque,
                               prd.Descricao,
                               est.DataAlteracao,
                               pes.Nome
                           };

            return Ok(estoques.FirstOrDefault());
        }

        [HttpGet("GetLastCodigoEstoque")]
        public IActionResult GetLastCodigoEstoque()
        {
            var maiorCodigoEstoque = _context.Estoque
                               .Max(est => (int?)est.CodigoEstoque) ?? 0;

            return Ok(maiorCodigoEstoque);
        }
    }
}
