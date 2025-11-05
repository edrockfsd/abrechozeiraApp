using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ABrechozeiraApp.Models;

namespace ABrechozeiraApp.Controllers;

[Route("api/[controller]")]
[ApiController]
[Microsoft.AspNetCore.Authorization.Authorize]
public class CaixaController : ControllerBase
{
    private readonly AbrechozeiraContext _context;
    public CaixaController(AbrechozeiraContext context) { _context = context; }

    [HttpPost("abrir")]
    public async Task<ActionResult<int>> Abrir([FromBody] Caixa caixa)
    {
        caixa.Id = 0; caixa.DataAbertura = DateTime.UtcNow;
        _context.Caixa.Add(caixa);
        await _context.SaveChangesAsync();
        return Ok(caixa.Id);
    }

    [HttpPost("fechar/{id}")]
    public async Task<ActionResult> Fechar(int id, [FromBody] decimal saldoFechamento)
    {
        var cx = await _context.Caixa.FindAsync(id);
        if (cx == null) return NotFound();
        cx.DataFechamento = DateTime.UtcNow;
        cx.SaldoFechamento = saldoFechamento;
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpGet("status/{id}")]
    public async Task<ActionResult<Caixa>> Status(int id)
    {
        var cx = await _context.Caixa.FindAsync(id);
        if (cx == null) return NotFound();
        return Ok(cx);
    }

    [HttpPost("suprimento")]
    public async Task<ActionResult> Suprimento([FromBody] CaixaMovimento mov)
    {
        mov.Id = 0; mov.Tipo = "Suprimento"; mov.DataRegistro = DateTime.UtcNow;
        _context.CaixaMovimento.Add(mov);
        await _context.SaveChangesAsync();
        return Ok();
    }

    [HttpPost("sangria")]
    public async Task<ActionResult> Sangria([FromBody] CaixaMovimento mov)
    {
        mov.Id = 0; mov.Tipo = "Sangria"; mov.DataRegistro = DateTime.UtcNow;
        _context.CaixaMovimento.Add(mov);
        await _context.SaveChangesAsync();
        return Ok();
    }
}

