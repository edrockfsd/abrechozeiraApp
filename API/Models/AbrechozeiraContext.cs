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

    public DbSet<ABrechozeiraApp.Models.ProdutoGrupo> ProdutoGrupo { get; set; } = default!;

    public DbSet<ABrechozeiraApp.Models.Origem> Origem { get; set; } = default!;

    public DbSet<ABrechozeiraApp.Models.Live> Live { get; set; } = default!;

    public DbSet<ABrechozeiraApp.Models.Endereco> Endereco { get; set; } = default!;

    public DbSet<ABrechozeiraApp.Models.TipoEndereco> TipoEndereco { get; set; } = default!;

    public DbSet<ABrechozeiraApp.Models.FormaPagamento> FormaPagamento { get; set; } = default!;

    public DbSet<ABrechozeiraApp.Models.CondicaoPagamento> CondicaoPagamento { get; set; } = default!;

    public DbSet<ABrechozeiraApp.Models.PedidoStatus> PedidoStatus { get; set; } = default!;

    public DbSet<ABrechozeiraApp.Models.Pedido> Pedido { get; set; } = default!;

    public DbSet<ABrechozeiraApp.Models.PedidoProduto> PedidoProduto { get; set; } = default!;

    public DbSet<ABrechozeiraApp.Models.ComentarioLive> ComentarioLive { get; set; } = default!;

    public DbSet<ABrechozeiraApp.Models.LiveSession> LiveSession { get; set; } = default!;

    public DbSet<ABrechozeiraApp.Models.RolePermission> RolePermission { get; set; } = default!;
    public DbSet<ABrechozeiraApp.Models.Permission> Permission { get; set; } = default!;
    public DbSet<ABrechozeiraApp.Models.Role> Role { get; set; } = default!;
    public DbSet<ABrechozeiraApp.Models.UserRole> UserRole { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        // ==========================================
        // CONFIGURAÇÕES DA ENTIDADE USER
        // ==========================================
        modelBuilder.Entity<User>(entity =>
        {
            // Índices
            entity.HasIndex(u => u.Email).IsUnique();
            entity.HasIndex(u => u.IsActive);
            entity.HasIndex(u => u.CreatedAt);

            // Relacionamento Many-to-Many com Role através de UserRole
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
        // CONFIGURAÇÕES DA ENTIDADE ROLE
        // ==========================================
        modelBuilder.Entity<Role>(entity =>
        {
            // Conversão do enum para string no banco
            entity.Property(r => r.Name)
                .HasConversion<string>();

            // Índices
            entity.HasIndex(r => r.Name).IsUnique();
            entity.HasIndex(r => r.IsActive);

            // Relacionamento Many-to-Many com Permission através de RolePermission
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
        // CONFIGURAÇÕES DA ENTIDADE PERMISSION
        // ==========================================
        modelBuilder.Entity<Permission>(entity =>
        {
            // Índices
            entity.HasIndex(p => p.Name).IsUnique();
            entity.HasIndex(p => p.Resource);
            entity.HasIndex(p => p.Action);
            entity.HasIndex(p => p.IsActive);
        });

        // ==========================================
        // DADOS INICIAIS (SEED DATA)
        // ==========================================
        //SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // Seed Permissions
        modelBuilder.Entity<Permission>().HasData(
            new Permission { Id = 1, Name = "produtos_create", Description = "Criar produtos", Resource = "produtos", Action = "CREATE" },
            new Permission { Id = 2, Name = "produtos_read", Description = "Visualizar produtos", Resource = "produtos", Action = "READ" },
            new Permission { Id = 3, Name = "produtos_update", Description = "Atualizar produtos", Resource = "produtos", Action = "UPDATE" },
            new Permission { Id = 4, Name = "produtos_delete", Description = "Deletar produtos", Resource = "produtos", Action = "DELETE" },
            new Permission { Id = 5, Name = "estoque_manage", Description = "Gerenciar estoque", Resource = "estoque", Action = "CREATE,UPDATE" },
            new Permission { Id = 6, Name = "estoque_read", Description = "Visualizar estoque", Resource = "estoque", Action = "READ" },
            new Permission { Id = 7, Name = "pedidos_create", Description = "Criar pedidos", Resource = "pedidos", Action = "CREATE" },
            new Permission { Id = 8, Name = "pedidos_read", Description = "Visualizar pedidos", Resource = "pedidos", Action = "READ" },
            new Permission { Id = 9, Name = "clientes_create", Description = "Cadastrar clientes", Resource = "clientes", Action = "CREATE" },
            new Permission { Id = 10, Name = "clientes_read", Description = "Visualizar clientes", Resource = "clientes", Action = "READ" },
            new Permission { Id = 11, Name = "relatorios_read", Description = "Visualizar relatórios", Resource = "relatorios", Action = "READ" },
            new Permission { Id = 99, Name = "full_access", Description = "Acesso completo a todos os recursos", Resource = "*", Action = "*" }
        );

        // Seed Roles
        modelBuilder.Entity<Role>().HasData(
            new Role { Id = 1, Name = RoleType.ADMIN, Description = "Acesso total ao sistema" },
            new Role { Id = 2, Name = RoleType.MANAGER, Description = "Gerente com acesso a produtos, estoque e pedidos" },
            new Role { Id = 3, Name = RoleType.SELLER, Description = "Vendedor com acesso a vendas e visualização" },
            new Role { Id = 4, Name = RoleType.VIEWER, Description = "Apenas visualização de dados" }
        );

        // Seed RolePermissions
        modelBuilder.Entity<RolePermission>().HasData(
            // ADMIN
            new RolePermission { Id = 1, RoleId = 1, PermissionId = 99 },
            // MANAGER
            new RolePermission { Id = 2, RoleId = 2, PermissionId = 1 }, // produtos_create
            new RolePermission { Id = 3, RoleId = 2, PermissionId = 2 }, // produtos_read
            new RolePermission { Id = 4, RoleId = 2, PermissionId = 3 }, // produtos_update
            new RolePermission { Id = 5, RoleId = 2, PermissionId = 5 }, // estoque_manage
            new RolePermission { Id = 6, RoleId = 2, PermissionId = 7 }, // pedidos_create
                                                                         // SELLER
            new RolePermission { Id = 7, RoleId = 3, PermissionId = 2 }, // produtos_read
            new RolePermission { Id = 8, RoleId = 3, PermissionId = 6 }, // estoque_read
            new RolePermission { Id = 9, RoleId = 3, PermissionId = 7 }, // pedidos_create
            new RolePermission { Id = 10, RoleId = 3, PermissionId = 9 }, // clientes_create
                                                                          // VIEWER
            new RolePermission { Id = 11, RoleId = 4, PermissionId = 2 }, // produtos_read
            new RolePermission { Id = 12, RoleId = 4, PermissionId = 6 }, // estoque_read
            new RolePermission { Id = 13, RoleId = 4, PermissionId = 11 } // relatorios_read
        );
    }
}
