using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ABrechozeiraApp.Models;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace ABrechozeiraApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public class ProdutosController : ControllerBase
    {
        private readonly AbrechozeiraContext _context;

        public ProdutosController(AbrechozeiraContext context)
        {
            _context = context;
        }

        // GET: api/Produtos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Produto>>> GetProdutos()
        {
            return await _context.Produto.ToListAsync();
        }

        // GET: api/Produtos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Produto>> GetProduto(int id)
        {
            var produto = await _context.Produto.FindAsync(id);

            if (produto == null)
            {
                return new Produto() { Descricao = "Produto não encontrado", Id = 0 };
            }

            return produto;
        }

        // PUT: api/Produtos/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduto(int id, Produto produto)
        {
            if (id != produto.Id)
            {
                return BadRequest();
            }

            _context.Entry(produto).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProdutoExists(id))
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

        // POST: api/Produtos
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Produto>> PostProduto(Produto produto)
        {
            _context.Produto.Add(produto);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetProduto", new { id = produto.Id }, produto);
        }

        // DELETE: api/Produtos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduto(int id)
        {
            var produto = await _context.Produto.FindAsync(id);
            if (produto == null)
            {
                return NotFound();
            }

            _context.Produto.Remove(produto);
            await _context.SaveChangesAsync();

            return NoContent();
        }
        [HttpGet]
        private bool ProdutoExists(int id)
        {
            return _context.Produto.Any(e => e.Id == id);
        }


        [HttpGet("ProdutoExistsByCodigoEstoque")]
        public ActionResult<bool> ProdutoExistsByCodigoEstoque(int codigoEstoque)
        {
            var produto = (from prd in _context.Produto
                           join est in _context.Estoque on prd.Id equals est.ProdutoId
                           where est.CodigoEstoque == codigoEstoque
                           select prd).FirstOrDefault();

            if (produto == null)
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        [HttpGet("GetProdutoByCodigoEstoque")]
        public ActionResult<Produto> GetProdutoByCodigoEstoque(int codigoEstoque)
        {
            try
            {
                var produto = (from prd in _context.Produto
                               join est in _context.Estoque on prd.Id equals est.ProdutoId
                               join pgr in _context.ProdutoGrupo on prd.GrupoID equals pgr.Id
                               where est.CodigoEstoque == codigoEstoque
                               select new { 
                                   prd.Id,
                                   prd.Descricao,
                                   prd.Condicao,
                                   prd.PrecoVenda,
                                   prd.Marca,
                                   prd.Tamanho,
                                   Categoria = pgr.Descricao
                               
                               }).FirstOrDefault();

                if (produto == null)
                {
                    return new Produto() { Descricao = "Produto não encontrado", Id = 0 };
                }

                return Ok(produto);
            }
            catch (Exception)
            {
                return new Produto() { Id = 0, Descricao = "Produto não encontrado" };
            }
            
        }

        [HttpGet("GetProdutosCompleto")]
        public IActionResult GetProdutosCompleto()
        {
            var produtos = from prd in _context.Produto
                           join pst in _context.ProdutoStatus on prd.StatusId equals pst.Id
                           join est0 in _context.Estoque on prd.Id equals est0.ProdutoId into estoqueJoin
                           from est in estoqueJoin.DefaultIfEmpty()
                           join perf0 in _context.ProdutoPerfil on prd.PerfilID equals perf0.Id into perfJoin
                           from perf in perfJoin.DefaultIfEmpty()
                           join mrc0 in _context.ProdutoMarca on prd.MarcaId equals mrc0.Id into marcaJoin
                           from mrc in marcaJoin.DefaultIfEmpty()
                           select new
                           {
                               prd.Id,
                               prd.DataAlteracao,
                               prd.DataCompra,
                               prd.GeneroID,
                               prd.GrupoID,
                               prd.PerfilID,
                               Perfil = perf != null ? perf.Descricao : null,
                               prd.MarcaId,
                               Marca = mrc != null ? mrc.Descricao : null,
                               est.CodigoEstoque,
                               prd.Descricao,
                               prd.Tamanho,
                               prd.Origem,
                               prd.PrecoCusto,
                               prd.PrecoVenda,
                               prd.StatusId
                         };

            return Ok(produtos.ToList());
        }

        [HttpGet("GetProdutoParaArremate")]
        public ActionResult<Produto> GetProdutoParaArremate(int codigoEstoque)
        {
            try
            {
                var produto = (from prd in _context.Produto
                               join est in _context.Estoque on prd.Id equals est.ProdutoId
                               where est.CodigoEstoque == codigoEstoque
                               select new
                               {
                                   prd.Id,
                                   prd.Descricao,
                                   prd.PrecoVenda
                               }).FirstOrDefault();

                if (produto == null)
                {
                    return new Produto() { Descricao = "Produto não encontrado", PrecoVenda = 0 };                    
                }

                return new Produto() { Id = produto.Id, Descricao = produto.Descricao, PrecoVenda = produto.PrecoVenda};
            }
            catch (Exception)
            {
                return new Produto() { Descricao = "Erro ao buscar produto"};
            }
        }
    }
}
