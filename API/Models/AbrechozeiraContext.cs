using System;
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
    public virtual DbSet<User> User { get; set; }
    public virtual DbSet<Venda> Venda { get; set; }
    public virtual DbSet<ProdutoStatus> ProdutoStatus { get; set; }
    public virtual DbSet<ProdutoMarca> ProdutoMarca { get; set; }
    public virtual DbSet<ProdutoPerfil> ProdutoPerfil { get; set; }
    public virtual DbSet<Arremate> Arremate { get; set; }
    public virtual DbSet<Estoque> Estoque { get; set; }
    public DbSet<ProdutoGrupo> ProdutoGrupo { get; set; } = default!;
    public DbSet<Origem> Origem { get; set; } = default!;
    public DbSet<Live> Live { get; set; } = default!;
    public DbSet<Endereco> Endereco { get; set; } = default!;
    public DbSet<TipoEndereco> TipoEndereco { get; set; } = default!;
    public DbSet<FormaPagamento> FormaPagamento { get; set; } = default!;
    public DbSet<CondicaoPagamento> CondicaoPagamento { get; set; } = default!;
    public DbSet<PedidoStatus> PedidoStatus { get; set; } = default!;
    public DbSet<Pedido> Pedido { get; set; } = default!;
    public DbSet<PedidoProduto> PedidoProduto { get; set; } = default!;
    public DbSet<ComentarioLive> ComentarioLive { get; set; } = default!;
    public DbSet<LiveSession> LiveSession { get; set; } = default!;
    public DbSet<RolePermission> RolePermission { get; set; } = default!;
    public DbSet<Permission> Permission { get; set; } = default!;
    public DbSet<Role> Role { get; set; } = default!;
    public DbSet<UserRole> UserRole { get; set; } = default!;

    // PDV
    public DbSet<VendaPdv> VendaPdv { get; set; } = default!;
    public DbSet<VendaPdvItem> VendaPdvItem { get; set; } = default!;
    public DbSet<VendaPdvPagamento> VendaPdvPagamento { get; set; } = default!;
    public DbSet<Caixa> Caixa { get; set; } = default!;
    public DbSet<CaixaMovimento> CaixaMovimento { get; set; } = default!;
    public DbSet<FormaPagamentoConfigPDV> FormaPagamentoConfigPDV { get; set; } = default!;

    // NFC-e
    public DbSet<EmpresaFiscal> EmpresaFiscal { get; set; } = default!;
    public DbSet<Nfce> Nfce { get; set; } = default!;
    public DbSet<NfceItem> NfceItem { get; set; } = default!;
    public DbSet<NfcePagamento> NfcePagamento { get; set; } = default!;

    // Envio em Lote (Superfrete/InfinitePay)
    public DbSet<EnvioLoteMap> EnvioLoteMap { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ==========================================
        // CONFIGURACOES DA ENTIDADE USER
        // ==========================================
        modelBuilder.Entity<User>(entity =>
        {
            // Indices
            entity.HasIndex(u => u.Email).IsUnique();
            entity.HasIndex(u => u.IsActive);
            entity.HasIndex(u => u.CreatedAt);

            // Relacionamento com Pessoa (obrigatorio)
            entity.HasOne(u => u.Pessoa)
                .WithOne(p => p.User)
                .HasForeignKey<User>(u => u.PessoaId)
                .OnDelete(DeleteBehavior.Restrict);

            // Relacionamento Many-to-Many com Role atraves de UserRole
            entity
                .HasMany(u => u.Roles)
                .WithMany(r => r.Users)
                .UsingEntity<UserRole>(
                    j => j
                        .HasOne(ur => ur.Role)
                        .WithMany(r => r.UserRoles)
                        .HasForeignKey(ur => ur.RoleId)
                        .OnDelete(DeleteBehavior.Cascade),
                    j => j
                        .HasOne(ur => ur.User)
                        .WithMany(u => u.UserRoles)
                        .HasForeignKey(ur => ur.UserId)
                        .OnDelete(DeleteBehavior.Cascade),
                    j =>
                    {
                        j.HasKey(ur => ur.Id);
                        j.HasIndex(ur => new { ur.UserId, ur.RoleId }).IsUnique();
                    });
        });

        // ==========================================
        // CONFIGURACOES DA ENTIDADE ROLE
        // ==========================================
        modelBuilder.Entity<Role>(entity =>
        {
            // Conversao do enum para string no banco
            entity.Property(r => r.Name)
                .HasConversion<string>();

            // Indices
            entity.HasIndex(r => r.Name).IsUnique();
            entity.HasIndex(r => r.IsActive);

            // Relacionamento Many-to-Many com Permission atraves de RolePermission
            entity
                .HasMany(r => r.Permissions)
                .WithMany(p => p.Roles)
                .UsingEntity<RolePermission>(
                    j => j
                        .HasOne(rp => rp.Permission)
                        .WithMany(p => p.RolePermissions)
                        .HasForeignKey(rp => rp.PermissionId)
                        .OnDelete(DeleteBehavior.Cascade),
                    j => j
                        .HasOne(rp => rp.Role)
                        .WithMany(r => r.RolePermissions)
                        .HasForeignKey(rp => rp.RoleId)
                        .OnDelete(DeleteBehavior.Cascade),
                    j =>
                    {
                        j.HasKey(rp => rp.Id);
                        j.HasIndex(rp => new { rp.RoleId, rp.PermissionId }).IsUnique();
                    });
        });

        // ==========================================
        // CONFIGURACOES DA ENTIDADE PERMISSION
        // ==========================================
        modelBuilder.Entity<Permission>(entity =>
        {
            // Indices
            entity.HasIndex(p => p.Name).IsUnique();
            entity.HasIndex(p => p.Resource);
            entity.HasIndex(p => p.Action);
            entity.HasIndex(p => p.IsActive);
        });
    }
}
