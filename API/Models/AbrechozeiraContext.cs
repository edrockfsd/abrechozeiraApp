﻿using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;
using ABrechozeiraApp.Models;

namespace ABrechozeiraApp.Models;

public partial class AbrechozeiraContext : DbContext
{
    public AbrechozeiraContext(DbContextOptions<AbrechozeiraContext> options)
        : base(options)
    {
    }

    public virtual DbSet<NivelAcesso> NivelAcesso { get; set; }

    public virtual DbSet<Pessoa> Pessoa { get; set; }

    public virtual DbSet<PessoaCategoria> PessoaCategoria { get; set; }

    public virtual DbSet<PessoaTipo> PessoaTipo { get; set; }

    public virtual DbSet<PessoaGenero> PessoaGenero { get; set; }

    public virtual DbSet<PessoaPerfil> PessoaPerfil { get; set; }

    public virtual DbSet<PessoaStatus> PessoaStatus { get; set; }

    public virtual DbSet<Produto> Produto { get; set; }

    public virtual DbSet<Usuario> Usuario { get; set; }

    public virtual DbSet<Venda> Venda { get; set; }

    public virtual DbSet<ProdutoStatus> ProdutoStatus { get; set; }

    public virtual DbSet<ProdutoMarca> ProdutoMarca { get; set; }

    public virtual DbSet<ProdutoPerfil> ProdutoPerfil { get; set; }

    public virtual DbSet<Arremate> Arremate { get; set; }

    public virtual DbSet<Estoque> Estoque { get; set; }

    public DbSet<ABrechozeiraApp.Models.ProdutoGrupo> ProdutoGrupo { get; set; } = default!;

    public DbSet<ABrechozeiraApp.Models.Origem> Origem { get; set; } = default!;

    public DbSet<ABrechozeiraApp.Models.Live> Live { get; set; } = default!;

    public DbSet<ABrechozeiraApp.Models.Endereco> Endereco { get; set; } = default!;
    
    public DbSet<ABrechozeiraApp.Models.TipoEndereco> TipoEndereco { get; set; } = default!;

}
