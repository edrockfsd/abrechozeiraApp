using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ABrechozeiraApp.Models;
using System.Security.Claims;

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
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        caixa.Id = 0;
        caixa.UsuarioId = userId.Value;
        caixa.DataAbertura = DateTime.UtcNow;
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

    [HttpGet("aberto")]
    public async Task<ActionResult<Caixa>> GetAberto()
    {
        var userId = GetCurrentUserId();
        if (userId == null) return Unauthorized();

        var caixa = await _context.Caixa
            .Where(c => c.UsuarioId == userId.Value && c.DataFechamento == null)
            .OrderByDescending(c => c.DataAbertura)
            .FirstOrDefaultAsync();

        if (caixa == null) return NotFound();
        return Ok(caixa);
    }

    [HttpGet("{id}/movimentos")]
    public async Task<ActionResult<IEnumerable<CaixaMovimento>>> GetMovimentos(int id)
    {
        if (!await _context.Caixa.AnyAsync(c => c.Id == id))
        {
            return NotFound();
        }

        var movimentos = await _context.CaixaMovimento
            .Where(m => m.CaixaId == id)
            .OrderByDescending(m => m.DataRegistro)
            .ToListAsync();

        return Ok(movimentos);
    }

    private int? GetCurrentUserId()
    {
        if (User?.Identity?.IsAuthenticated != true) return null;
        var claim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("id");
        if (claim == null) return null;
        return int.TryParse(claim.Value, out var id) ? id : (int?)null;
    }
}
