using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ABrechozeiraApp.Models;
using ABrechozeiraApp.Services;

namespace ABrechozeiraApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Microsoft.AspNetCore.Authorization.Authorize]
    public class VendasController : ControllerBase
    {
        private readonly AbrechozeiraContext _context;
        private readonly VendaService _vendaService;
        private readonly NfceService _nfceService;

        public VendasController(AbrechozeiraContext context, VendaService vendaService, NfceService nfceService)
        {
            _context = context;
            _vendaService = vendaService;
            _nfceService = nfceService;
        }

        /// <summary>
        /// Listagem de vendas com filtros (Live, data, status, cliente)
        /// </summary>
        [HttpGet("listagem")]
        public async Task<IActionResult> GetListagem(
            [FromQuery] int? liveId,
            [FromQuery] string? status,
            [FromQuery] DateTime? inicio,
            [FromQuery] DateTime? fim,
            [FromQuery] int? clienteId,
            [FromQuery] int limite = 100)
        {
            var q = _context.Venda.AsQueryable();

            if (liveId.HasValue)
                q = q.Where(v => v.LiveId == liveId.Value);
            if (!string.IsNullOrWhiteSpace(status))
                q = q.Where(v => v.Status == status);
            if (inicio.HasValue)
                q = q.Where(v => v.DataVenda >= inicio.Value);
            if (fim.HasValue)
                q = q.Where(v => v.DataVenda <= fim.Value);
            if (clienteId.HasValue)
                q = q.Where(v => v.ClienteId == clienteId.Value);

            var take = limite > 0 && limite <= 500 ? limite : 100;

            var vendas = await q
                .OrderByDescending(v => v.DataVenda)
                .Take(take)
                .Select(v => new
                {
                    v.Id,
                    v.PedidoId,
                    v.Status,
                    v.ValorBruto,
                    v.Desconto,
                    v.ValorTotal,
                    v.DataVenda,
                    v.DataPagamento,
                    v.LiveId,
                    v.FormaPagamentoId,
                    ClienteNome = v.Cliente != null ? v.Cliente.Nome : null,
                    ClienteNick = v.Cliente != null ? v.Cliente.NickName : null,
                    LiveTitulo = v.Live != null ? v.Live.Titulo : null,
                    FormaPagamento = v.FormaPagamento != null ? v.FormaPagamento.Descricao : null,
                    TemNfce = _context.Nfce.Any(n => n.VendaId == v.Id && n.Status != "Cancelada"),
                    NfceStatus = _context.Nfce
                        .Where(n => n.VendaId == v.Id)
                        .OrderByDescending(n => n.DataEmissao)
                        .Select(n => n.Status)
                        .FirstOrDefault(),
                    QtdItens = _context.PedidoProduto.Count(pp => pp.PedidoId == v.PedidoId)
                })
                .ToListAsync();

            return Ok(vendas);
        }

        /// <summary>
        /// Detalhes de uma venda com itens do pedido
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetVenda(int id)
        {
            var venda = await _context.Venda
                .Include(v => v.Cliente)
                .Include(v => v.Live)
                .Include(v => v.FormaPagamento)
                .Include(v => v.CondicaoPagamento)
                .Include(v => v.Origem)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (venda == null)
                return NotFound();

            var itens = await _context.PedidoProduto
                .Include(pp => pp.Produto)
                .Where(pp => pp.PedidoId == venda.PedidoId)
                .Select(pp => new
                {
                    pp.Id,
                    pp.ProdutoId,
                    ProdutoDescricao = pp.Produto != null ? pp.Produto.Descricao : "Produto",
                    pp.Quantidade,
                    pp.ValorFinalProduto,
                    pp.DescontoValor
                })
                .ToListAsync();

            var nfce = await _context.Nfce
                .Where(n => n.VendaId == id)
                .Select(n => new { n.Id, n.Numero, n.ChaveAcesso, n.Status, n.DataEmissao })
                .FirstOrDefaultAsync();

            return Ok(new { venda, itens, nfce });
        }

        /// <summary>
        /// Gera uma Venda a partir de um Pedido confirmado
        /// </summary>
        [HttpPost("gerar-de-pedido/{pedidoId}")]
        public async Task<ActionResult<object>> GerarDePedido(int pedidoId)
        {
            try
            {
                var venda = await _vendaService.GerarVendaDePedidoAsync(pedidoId);
                return Ok(new
                {
                    mensagem = "Venda gerada com sucesso",
                    vendaId = venda.Id,
                    pedidoId = venda.PedidoId,
                    valorTotal = venda.ValorTotal
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { erro = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { erro = ex.Message });
            }
        }

        /// <summary>
        /// Emite NFC-e em lote para vendas selecionadas
        /// </summary>
        [HttpPost("emitir-nfce-lote")]
        public async Task<ActionResult<object>> EmitirNfceLote([FromBody] EmitirNfceLoteRequest request)
        {
            if (request.VendaIds == null || request.VendaIds.Count == 0)
                return BadRequest(new { erro = "Nenhuma venda selecionada." });

            var resultados = new List<object>();
            var erros = new List<object>();
            int sucessos = 0;

            foreach (var vendaId in request.VendaIds)
            {
                try
                {
                    var nfce = await _nfceService.EmitirNfcePorVendaAsync(vendaId);
                    sucessos++;
                    resultados.Add(new
                    {
                        vendaId,
                        nfceId = nfce.Id,
                        numero = nfce.Numero,
                        chaveAcesso = nfce.ChaveAcesso,
                        status = nfce.Status
                    });
                }
                catch (Exception ex)
                {
                    erros.Add(new { vendaId, erro = ex.Message });
                }
            }

            return Ok(new
            {
                mensagem = $"NFC-e em lote: {sucessos} emitidas, {erros.Count} erros",
                sucessos,
                totalErros = erros.Count,
                resultados,
                erros
            });
        }

        // GET: api/Vendas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Venda>>> GetVendas()
        {
            return await _context.Venda.ToListAsync();
        }

        // PUT: api/Vendas/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutVenda(int id, Venda venda)
        {
            if (id != venda.Id)
            {
                return BadRequest();
            }

            _context.Entry(venda).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!VendaExists(id))
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

        // POST: api/Vendas
        [HttpPost]
        public async Task<ActionResult<Venda>> PostVenda(Venda venda)
        {
            _context.Venda.Add(venda);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetVenda", new { id = venda.Id }, venda);
        }

        // DELETE: api/Vendas/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVenda(int id)
        {
            var venda = await _context.Venda.FindAsync(id);
            if (venda == null)
            {
                return NotFound();
            }

            _context.Venda.Remove(venda);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool VendaExists(int id)
        {
            return _context.Venda.Any(e => e.Id == id);
        }
    }

    public class EmitirNfceLoteRequest
    {
        public List<int> VendaIds { get; set; } = new();
    }
}
