using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ABrechozeiraApp.Models;

namespace ABrechozeiraApp.Controllers;

[Route("api/[controller]")]
[ApiController]
[Microsoft.AspNetCore.Authorization.Authorize]
public class DashboardController : ControllerBase
{
    private readonly AbrechozeiraContext _context;
    
    public DashboardController(AbrechozeiraContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<object>> GetDashboard()
    {
        var hoje = DateTime.UtcNow.Date;
        var inicioSemana = hoje.AddDays(-(int)hoje.DayOfWeek);
        var inicioMes = new DateTime(hoje.Year, hoje.Month, 1);
        var seteDiasAtras = hoje.AddDays(-6);

        // Vendas finalizadas
        var vendasFinalizadas = _context.VendaPdv.Where(v => v.Status == "Finalizada");

        // Total vendas hoje
        var vendasHoje = await vendasFinalizadas
            .Where(v => v.DataVenda.Date == hoje)
            .SumAsync(v => v.ValorLiquido);

        // Total vendas semana
        var vendasSemana = await vendasFinalizadas
            .Where(v => v.DataVenda.Date >= inicioSemana)
            .SumAsync(v => v.ValorLiquido);

        // Total vendas mês
        var vendasMes = await vendasFinalizadas
            .Where(v => v.DataVenda.Date >= inicioMes)
            .SumAsync(v => v.ValorLiquido);

        // Vendas últimos 7 dias (para gráfico) - simplificado para evitar erros EF
        var vendasUltimos7DiasRaw = await vendasFinalizadas
            .Where(v => v.DataVenda.Date >= seteDiasAtras)
            .Select(v => new { v.DataVenda, v.ValorLiquido })
            .ToListAsync();

        var vendasAgrupadas = vendasUltimos7DiasRaw
            .GroupBy(v => v.DataVenda.Date)
            .Select(g => new { data = g.Key, total = g.Sum(v => v.ValorLiquido) })
            .ToDictionary(x => x.data, x => x.total);

        // Preencher dias sem vendas
        var vendasPorDia = new List<object>();
        for (int i = 0; i < 7; i++)
        {
            var dia = seteDiasAtras.AddDays(i);
            vendasAgrupadas.TryGetValue(dia, out var total);
            vendasPorDia.Add(new { 
                data = dia.ToString("yyyy-MM-dd"), 
                diaSemana = dia.ToString("ddd"),
                total = total
            });
        }

        // Top 5 produtos mais vendidos (últimos 30 dias) - simplificado
        var trintaDiasAtras = hoje.AddDays(-30);
        var vendasIdsRecentes = await vendasFinalizadas
            .Where(v => v.DataVenda >= trintaDiasAtras)
            .Select(v => v.Id)
            .ToListAsync();

        var produtosMaisVendidos = await _context.VendaPdvItem
            .Where(i => vendasIdsRecentes.Contains(i.VendaPdvId))
            .GroupBy(i => new { i.ProdutoId, i.DescricaoItem })
            .Select(g => new { 
                produtoId = g.Key.ProdutoId,
                descricao = g.Key.DescricaoItem, 
                quantidade = g.Sum(x => x.Quantidade),
                totalVendido = g.Sum(x => x.Total)
            })
            .OrderByDescending(x => x.quantidade)
            .Take(5)
            .ToListAsync();

        // Estoque baixo (quantidade <= 3)
        var estoqueBaixo = await _context.Estoque
            .Where(e => e.Quantidade <= 3 && e.Quantidade > 0)
            .Join(_context.Produto, e => e.ProdutoId, p => p.Id, (e, p) => new {
                id = e.Id,
                produtoId = e.ProdutoId,
                descricao = p.Descricao,
                quantidade = e.Quantidade,
                codigoEstoque = e.CodigoEstoque
            })
            .Take(10)
            .ToListAsync();

        // Próxima live
        var proximaLive = await _context.Live
            .Where(l => l.DataLive >= DateTime.UtcNow)
            .OrderBy(l => l.DataLive)
            .Select(l => new { titulo = l.Titulo, data = l.DataLive })
            .FirstOrDefaultAsync();

        // Contadores gerais
        var totalProdutos = await _context.Produto.CountAsync();
        var totalClientes = await _context.Pessoa.Where(p => p.PessoaCategoriaId == 1).CountAsync();
        var vendasHojeCount = await vendasFinalizadas.Where(v => v.DataVenda.Date == hoje).CountAsync();

        return Ok(new {
            vendasHoje,
            vendasSemana,
            vendasMes,
            vendasHojeCount,
            vendasUltimos7Dias = vendasPorDia,
            produtosMaisVendidos,
            estoqueBaixo,
            proximaLive,
            totalProdutos,
            totalClientes
        });
    }
}
