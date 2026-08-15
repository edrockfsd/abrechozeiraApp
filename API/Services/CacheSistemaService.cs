using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ABrechozeiraApp.Models;
using Microsoft.EntityFrameworkCore;

namespace ABrechozeiraApp.Services;

public class ItemDominio
{
    public int Id { get; set; }
    public string Nome { get; set; } = string.Empty;
}

/// <summary>
/// Cache das tabelas de domínio usadas pelo prompt de IA para desmembrar descrições de produtos
/// </summary>
public class CacheSistemaService
{
    private readonly AbrechozeiraContext _context;

    public List<ItemDominio> Marcas { get; private set; } = new();
    public List<ItemDominio> Grupos { get; private set; } = new();
    public List<ItemDominio> Generos { get; private set; } = new();
    public List<ItemDominio> Perfis { get; private set; } = new();

    public CacheSistemaService(AbrechozeiraContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Carrega todas as tabelas de domínio do banco
    /// </summary>
    public async Task CarregarAsync()
    {
        Marcas = await _context.ProdutoMarca
            .Select(m => new ItemDominio { Id = m.Id, Nome = m.Descricao })
            .ToListAsync();

        Grupos = await _context.ProdutoGrupo
            .Select(g => new ItemDominio { Id = g.Id, Nome = g.Descricao })
            .ToListAsync();

        Generos = await _context.PessoaGenero
            .Select(g => new ItemDominio { Id = g.Id, Nome = g.Descricao })
            .ToListAsync();

        Perfis = await _context.ProdutoPerfil
            .Select(p => new ItemDominio { Id = p.Id, Nome = p.Descricao })
            .ToListAsync();
    }
}
