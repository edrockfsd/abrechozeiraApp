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
    [Microsoft.AspNetCore.Authorization.Authorize]
    public class PedidoController : ControllerBase
    {
        private readonly AbrechozeiraContext _context;

        public PedidoController(AbrechozeiraContext context)
        {
            _context = context;
        }

        // GET: api/Pedido
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Pedido>>> GetPedido()
        {
            return await _context.Pedido.ToListAsync();
        }

        // GET: api/Pedido/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Pedido>> GetPedido(int id)
        {
            var Pedido = await _context.Pedido.FindAsync(id);

            if (Pedido == null)
            {
                return NotFound();
            }

            return Pedido;
        }

        // PUT: api/Pedido/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPedido(int id, Pedido Pedido)
        {
            if (id != Pedido.Id)
            {
                return BadRequest();
            }

            _context.Entry(Pedido).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PedidoExists(id))
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

        // POST: api/Pedido
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Pedido>> PostPedido(Pedido Pedido)
        {
            _context.Pedido.Add(Pedido);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPedido", new { id = Pedido.Id }, Pedido);
        }

        // DELETE: api/Pedido/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePedido(int id)
        {
            var Pedido = await _context.Pedido.FindAsync(id);
            if (Pedido == null)
            {
                return NotFound();
            }

            _context.Pedido.Remove(Pedido);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PedidoExists(int id)
        {
            return _context.Pedido.Any(e => e.Id == id);
        }

        /// <summary>
        /// Obtém o último valor do campo PedidoCodigo (valor mais alto) da tabela Pedido.
        /// </summary>
        /// <returns>O último valor de PedidoCodigo ou 0 se não existirem registros.</returns>
        [HttpGet("ObterUltimoPedidoCodigo")]
        public int ObterUltimoPedidoCodigo()
        {
            // Busca o valor máximo do campo PedidoCodigo
            var ultimoCodigo = _context.Pedido
                .Max(p => p.PedidoCodigo);

            return ultimoCodigo;
        }

        /// <summary>
        /// Gera um novo código de pedido com base no último código existente.
        /// </summary>
        /// <returns>O novo código de pedido.</returns>
        [HttpGet("GerarNovoPedidoCodigo")]
        public int GerarNovoPedidoCodigo()
        {
            var ultimoCodigo = ObterUltimoPedidoCodigo();
            return ultimoCodigo + 1;
        }

        [HttpGet("GetListaPedido")]
        public IActionResult GetListaPedido()
        {
            var pedidos = from ped in _context.Pedido
                           join pst in _context.PedidoStatus on ped.PedidoStatusID equals pst.Id
                           join pes in _context.Pessoa on ped.ClienteID equals pes.Id
                           join ender in _context.Endereco on ped.EnderecoEntregaID equals ender.Id into enderJoin
                           from ender in enderJoin.DefaultIfEmpty()
                           orderby ped.Id descending
                           select new
                           {
                               ped.Id,
                               ped.PedidoCodigo,
                               ClienteNome = pes.Nome,
                               ClienteNick = pes.NickName,
                               DataPedido = ped.DataLancamento,
                               ValorTotal = ped.ValorTotal ?? 0,
                               ValorFrete = ped.ValorFrete ?? 0,
                               Status = pst.Descricao,
                               CEP = ender == null ? null : ender.CEP,
                               Endereco = ender == null
                                    ? ""
                                    : ((ender.Logradouro ?? "")
                                       + ((ender.Bairro != null && ender.Bairro != "") ? ", " + ender.Bairro : "")
                                       + (((ender.Localidade != null && ender.Localidade != "") || (ender.Estado != null && ender.Estado != ""))
                                            ? (" - " + (ender.Localidade ?? "") + ((ender.Estado != null && ender.Estado != "") ? "/" + ender.Estado : ""))
                                            : ""))
                           };

            return Ok(pedidos.ToList());
        }
    }
}
