using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ABrechozeiraApp.Models;

namespace ABrechozeiraApp.Controllers;

[Route("api/[controller]")]
[ApiController]
[Microsoft.AspNetCore.Authorization.Authorize]
public class VendasPdvController : ControllerBase
{
    private readonly AbrechozeiraContext _context;
    public VendasPdvController(AbrechozeiraContext context)
    {
        _context = context;
    }

    [HttpGet("config")]
    public async Task<ActionResult<object>> GetConfig()
    {
        var formas = await _context.FormaPagamentoConfigPDV
            .Include(c => c.FormaPagamento!)
            .Select(c => new
            {
                id = c.FormaPagamentoId,
                descricao = c.FormaPagamento!.Descricao,
                exibirNoPDV = c.ExibirNoPDV,
                permiteParcelamento = c.PermiteParcelamento,
                maxParcelas = c.MaxParcelas,
                taxaAdmPerc = c.TaxaAdmPerc
            })
            .ToListAsync();

        var condicoes = await _context.CondicaoPagamento
            .Select(cp => new { id = cp.Id, descricao = cp.Descricao })
            .ToListAsync();

        return Ok(new { formasPagamento = formas, condicaoPagamento = condicoes });
    }

    [HttpPost]
    public async Task<ActionResult<object>> AbrirVenda([FromBody] VendaPdv venda)
    {
        venda.Id = 0;
        venda.Status = "Aberta";
        venda.DataVenda = DateTime.UtcNow;
        venda.DataAlteracao = DateTime.UtcNow;
        _context.VendaPdv.Add(venda);
        await _context.SaveChangesAsync();
        // Padroniza a propriedade em camelCase para o front esperar "id"
        return Ok(new { id = venda.Id });
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<object>> GetVenda(int id)
    {
        var venda = await _context.VendaPdv.FindAsync(id);
        if (venda == null) return NotFound();

        var itens = await _context.VendaPdvItem.Where(i => i.VendaPdvId == id).ToListAsync();
        var pagamentos = await _context.VendaPdvPagamento.Where(p => p.VendaPdvId == id).ToListAsync();
        return Ok(new { venda, itens, pagamentos });
    }

    // Listagem simples: últimas vendas com totais
    [HttpGet]
    public async Task<ActionResult<IEnumerable<object>>> List([FromQuery] int? limit, [FromQuery] string? status, [FromQuery] DateTime? start, [FromQuery] DateTime? end)
    {
        var q = _context.VendaPdv.AsQueryable();
        if (!string.IsNullOrWhiteSpace(status))
        {
            q = q.Where(v => v.Status == status);
        }
        if (start.HasValue)
        {
            q = q.Where(v => v.DataVenda >= start.Value);
        }
        if (end.HasValue)
        {
            q = q.Where(v => v.DataVenda <= end.Value);
        }
        var take = limit.HasValue && limit.Value > 0 && limit.Value <= 500 ? limit.Value : 50;
        var data = await q
            .OrderByDescending(v => v.DataVenda)
            .Take(take)
            .Select(v => new
            {
                v.Id,
                v.Codigo,
                v.Status,
                v.DataVenda,
                v.ValorBruto,
                v.Desconto,
                v.ValorLiquido
            })
            .ToListAsync();
        return Ok(data);
    }

    [HttpPost("{id}/itens")]
    public async Task<ActionResult> AddItem(int id, [FromBody] VendaPdvItem item)
    {
        if (await _context.VendaPdv.FindAsync(id) is null) return NotFound();
        item.Id = 0; item.VendaPdvId = id;
        item.Total = (item.Quantidade * item.PrecoUnitario) - (item.DescontoValor ?? 0);
        _context.VendaPdvItem.Add(item);
        await RecalcularTotais(id);
        return Ok();
    }

    [HttpPut("{id}/itens/{itemId}")]
    public async Task<ActionResult> UpdateItem(int id, int itemId, [FromBody] VendaPdvItem item)
    {
        var current = await _context.VendaPdvItem.FirstOrDefaultAsync(i => i.Id == itemId && i.VendaPdvId == id);
        if (current == null) return NotFound();
        current.DescricaoItem = item.DescricaoItem;
        current.ProdutoId = item.ProdutoId;
        current.CodigoEstoque = item.CodigoEstoque;
        current.Quantidade = item.Quantidade;
        current.PrecoUnitario = item.PrecoUnitario;
        current.DescontoValor = item.DescontoValor;
        current.DescontoPerc = item.DescontoPerc;
        current.Total = (item.Quantidade * item.PrecoUnitario) - (item.DescontoValor ?? 0);
        await RecalcularTotais(id);
        return Ok();
    }

    [HttpDelete("{id}/itens/{itemId}")]
    public async Task<ActionResult> DeleteItem(int id, int itemId)
    {
        var item = await _context.VendaPdvItem.FirstOrDefaultAsync(i => i.Id == itemId && i.VendaPdvId == id);
        if (item == null) return NotFound();
        _context.VendaPdvItem.Remove(item);
        await RecalcularTotais(id);
        return Ok();
    }

    [HttpPost("{id}/pagamentos")]
    public async Task<ActionResult> AddPagamento(int id, [FromBody] VendaPdvPagamento pagamento)
    {
        if (await _context.VendaPdv.FindAsync(id) is null) return NotFound();
        pagamento.Id = 0; pagamento.VendaPdvId = id; pagamento.DataRegistro = DateTime.UtcNow;
        _context.VendaPdvPagamento.Add(pagamento);
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpDelete("{id}/pagamentos/{pgId}")]
    public async Task<ActionResult> DeletePagamento(int id, int pgId)
    {
        var pg = await _context.VendaPdvPagamento.FirstOrDefaultAsync(p => p.Id == pgId && p.VendaPdvId == id);
        if (pg == null) return NotFound();
        _context.VendaPdvPagamento.Remove(pg);
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpPost("{id}/finalizar")]
    public async Task<ActionResult> Finalizar(int id)
    {
        var venda = await _context.VendaPdv.FindAsync(id);
        if (venda == null) return NotFound();
        venda.Status = "Finalizada";
        venda.DataAlteracao = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        // Opcional: registrar movimento de caixa se houver CaixaId
        if (venda.CaixaId.HasValue)
        {
            var total = await _context.VendaPdvItem.Where(i => i.VendaPdvId == id).SumAsync(i => i.Total);
            _context.CaixaMovimento.Add(new CaixaMovimento
            {
                CaixaId = venda.CaixaId.Value,
                Tipo = "Venda",
                Valor = total,
                ReferenciaId = id,
                DataRegistro = DateTime.UtcNow
            });
            await _context.SaveChangesAsync();
        }
        return Ok();
    }

    [HttpPost("{id}/cancelar")]
    public async Task<ActionResult> Cancelar(int id)
    {
        var venda = await _context.VendaPdv.FindAsync(id);
        if (venda == null) return NotFound();
        venda.Status = "Cancelada";
        venda.DataAlteracao = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return Ok();
    }

    private async Task RecalcularTotais(int vendaId)
    {
        // Flush pendências (ADD/UPDATE/DELETE) antes do cálculo
        await _context.SaveChangesAsync();
        var venda = await _context.VendaPdv.FindAsync(vendaId);
        if (venda == null) return;
        var bruto = await _context.VendaPdvItem.Where(i => i.VendaPdvId == vendaId)
            .SumAsync(i => i.Quantidade * i.PrecoUnitario);
        var desc = await _context.VendaPdvItem.Where(i => i.VendaPdvId == vendaId)
            .SumAsync(i => (decimal?)(i.DescontoValor ?? 0));
        venda.ValorBruto = bruto;
        venda.Desconto = desc;
        venda.ValorLiquido = bruto - (desc ?? 0);
        venda.DataAlteracao = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }
}
