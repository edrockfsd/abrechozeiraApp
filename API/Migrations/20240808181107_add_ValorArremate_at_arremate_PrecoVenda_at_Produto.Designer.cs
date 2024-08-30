﻿// <auto-generated />
using System;
using ABrechozeiraApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace ABrechozeiraApp.Migrations
{
    [DbContext(typeof(AbrechozeiraContext))]
    [Migration("20240808181107_add_ValorArremate_at_arremate_PrecoVenda_at_Produto")]
    partial class add_ValorArremate_at_arremate_PrecoVenda_at_Produto
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.2")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            MySqlModelBuilderExtensions.AutoIncrementColumns(modelBuilder);

            modelBuilder.Entity("ABrechozeiraApp.Models.Arremate", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Arrematante")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.Property<DateTime>("DataAlteracao")
                        .HasColumnType("datetime(6)");

                    b.Property<DateTime>("DataArremate")
                        .HasColumnType("datetime(6)");

                    b.Property<int>("LiveId")
                        .HasColumnType("int");

                    b.Property<string>("Observacoes")
                        .HasColumnType("longtext");

                    b.Property<int>("ProdutoId")
                        .HasColumnType("int");

                    b.Property<int>("UsuarioModificacaoId")
                        .HasColumnType("int");

                    b.Property<decimal?>("ValorArremate")
                        .HasColumnType("decimal(65,30)");

                    b.HasKey("Id");

                    b.HasIndex("LiveId");

                    b.HasIndex("ProdutoId");

                    b.HasIndex("UsuarioModificacaoId");

                    b.ToTable("Arremate");
                });

            modelBuilder.Entity("ABrechozeiraApp.Models.Estoque", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<int?>("CodigoEstoque")
                        .HasColumnType("int");

                    b.Property<DateTime?>("DataAlteracao")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Localizacao")
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");

                    b.Property<int>("ProdutoId")
                        .HasColumnType("int");

                    b.Property<int>("Quantidade")
                        .HasColumnType("int");

                    b.Property<int>("UsuarioModificacaoId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("ProdutoId");

                    b.HasIndex("UsuarioModificacaoId");

                    b.ToTable("Estoque");
                });

            modelBuilder.Entity("ABrechozeiraApp.Models.Grupo", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Descricao")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");

                    b.HasKey("Id");

                    b.ToTable("Grupo");
                });

            modelBuilder.Entity("ABrechozeiraApp.Models.Live", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime>("DataAlteracao")
                        .HasColumnType("datetime(6)");

                    b.Property<DateTime>("DataLive")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Observacoes")
                        .HasColumnType("longtext");

                    b.Property<string>("Titulo")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.Property<int>("UsuarioModificacaoId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("UsuarioModificacaoId");

                    b.ToTable("Live");
                });

            modelBuilder.Entity("ABrechozeiraApp.Models.Marca", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Descricao")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");

                    b.HasKey("Id");

                    b.ToTable("Marca");
                });

            modelBuilder.Entity("ABrechozeiraApp.Models.NivelAcesso", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Descricao")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("varchar(200)");

                    b.HasKey("Id");

                    b.ToTable("NivelAcesso");
                });

            modelBuilder.Entity("ABrechozeiraApp.Models.Origem", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Descricao")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("varchar(200)");

                    b.HasKey("Id");

                    b.ToTable("Origem");
                });

            modelBuilder.Entity("ABrechozeiraApp.Models.Pessoa", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<DateTime?>("DataInclusao")
                        .HasColumnType("datetime(6)");

                    b.Property<DateTime?>("DataNascimento")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Email")
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");

                    b.Property<string>("NickName")
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.Property<string>("Nome")
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.Property<int>("PessoaCategoriaId")
                        .HasColumnType("int");

                    b.Property<int>("PessoaTipoId")
                        .HasColumnType("int");

                    b.Property<string>("Sexo")
                        .HasMaxLength(9)
                        .HasColumnType("varchar(9)");

                    b.Property<int>("StatusId")
                        .HasColumnType("int");

                    b.Property<string>("Telefone")
                        .HasMaxLength(13)
                        .HasColumnType("varchar(13)");

                    b.HasKey("Id");

                    b.HasIndex("PessoaCategoriaId");

                    b.HasIndex("PessoaTipoId");

                    b.HasIndex("StatusId");

                    b.ToTable("Pessoa");
                });

            modelBuilder.Entity("ABrechozeiraApp.Models.PessoaCategoria", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Descricao")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.HasKey("Id");

                    b.ToTable("PessoaCategoria");
                });

            modelBuilder.Entity("ABrechozeiraApp.Models.PessoaStatus", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Descricao")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.HasKey("Id");

                    b.ToTable("PessoaStatus");
                });

            modelBuilder.Entity("ABrechozeiraApp.Models.PessoaTipo", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Descricao")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.HasKey("Id");

                    b.ToTable("PessoaTipo");
                });

            modelBuilder.Entity("ABrechozeiraApp.Models.Produto", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<int?>("CodigoLive")
                        .HasColumnType("int");

                    b.Property<DateTime?>("DataAlteracao")
                        .HasColumnType("datetime(6)");

                    b.Property<DateTime?>("DataCompra")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Descricao")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");

                    b.Property<int>("GrupoID")
                        .HasColumnType("int");

                    b.Property<int?>("MarcaId")
                        .HasColumnType("int");

                    b.Property<string>("Origem")
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.Property<int?>("PessoaPertenceID")
                        .HasColumnType("int");

                    b.Property<decimal?>("PrecoCusto")
                        .HasColumnType("decimal(65,30)");

                    b.Property<decimal?>("PrecoVenda")
                        .HasColumnType("decimal(65,30)");

                    b.Property<int>("StatusId")
                        .HasColumnType("int");

                    b.Property<string>("Tamanho")
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.Property<int>("UsuarioModificacaoId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("GrupoID");

                    b.HasIndex("MarcaId");

                    b.HasIndex("PessoaPertenceID");

                    b.HasIndex("StatusId");

                    b.HasIndex("UsuarioModificacaoId");

                    b.ToTable("Produto");
                });

            modelBuilder.Entity("ABrechozeiraApp.Models.ProdutoStatus", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Descricao")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.HasKey("Id");

                    b.ToTable("ProdutoStatus");
                });

            modelBuilder.Entity("ABrechozeiraApp.Models.Usuario", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Login")
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.Property<int>("NivelAcessoID")
                        .HasColumnType("int");

                    b.Property<int>("PessoaID")
                        .HasColumnType("int");

                    b.Property<string>("Senha")
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.HasKey("Id");

                    b.HasIndex("NivelAcessoID");

                    b.HasIndex("PessoaID");

                    b.ToTable("Usuario");
                });

            modelBuilder.Entity("ABrechozeiraApp.Models.Venda", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<int>("ClienteId")
                        .HasColumnType("int");

                    b.Property<int?>("CodigoLive")
                        .HasColumnType("int");

                    b.Property<DateTime?>("DataAlteracao")
                        .HasColumnType("datetime(6)");

                    b.Property<DateTime?>("DataPagamento")
                        .HasColumnType("datetime(6)");

                    b.Property<DateTime?>("DataVenda")
                        .HasColumnType("datetime(6)");

                    b.Property<decimal?>("Desconto")
                        .HasColumnType("decimal(65,30)");

                    b.Property<int?>("LiveId")
                        .HasColumnType("int");

                    b.Property<int?>("OrdemVendaLive")
                        .HasColumnType("int");

                    b.Property<int?>("OrigemID")
                        .HasColumnType("int");

                    b.Property<int>("ProdutoId")
                        .HasColumnType("int");

                    b.Property<int>("Quantidade")
                        .HasColumnType("int");

                    b.Property<int>("UsuarioModificacaoId")
                        .HasColumnType("int");

                    b.Property<decimal>("ValorVenda")
                        .HasColumnType("decimal(65,30)");

                    b.HasKey("Id");

                    b.HasIndex("ClienteId");

                    b.HasIndex("LiveId");

                    b.HasIndex("OrigemID");

                    b.HasIndex("ProdutoId");

                    b.HasIndex("UsuarioModificacaoId");

                    b.ToTable("Venda");
                });

            modelBuilder.Entity("ABrechozeiraApp.Models.Arremate", b =>
                {
                    b.HasOne("ABrechozeiraApp.Models.Live", "Live")
                        .WithMany()
                        .HasForeignKey("LiveId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ABrechozeiraApp.Models.Produto", "Produto")
                        .WithMany()
                        .HasForeignKey("ProdutoId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ABrechozeiraApp.Models.Usuario", "UsuarioModificacao")
                        .WithMany()
                        .HasForeignKey("UsuarioModificacaoId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Live");

                    b.Navigation("Produto");

                    b.Navigation("UsuarioModificacao");
                });

            modelBuilder.Entity("ABrechozeiraApp.Models.Estoque", b =>
                {
                    b.HasOne("ABrechozeiraApp.Models.Produto", "Produto")
                        .WithMany()
                        .HasForeignKey("ProdutoId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ABrechozeiraApp.Models.Usuario", "UsuarioModificacao")
                        .WithMany()
                        .HasForeignKey("UsuarioModificacaoId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Produto");

                    b.Navigation("UsuarioModificacao");
                });

            modelBuilder.Entity("ABrechozeiraApp.Models.Live", b =>
                {
                    b.HasOne("ABrechozeiraApp.Models.Usuario", "UsuarioModificacao")
                        .WithMany()
                        .HasForeignKey("UsuarioModificacaoId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("UsuarioModificacao");
                });

            modelBuilder.Entity("ABrechozeiraApp.Models.Pessoa", b =>
                {
                    b.HasOne("ABrechozeiraApp.Models.PessoaCategoria", "PessoaCategoria")
                        .WithMany()
                        .HasForeignKey("PessoaCategoriaId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ABrechozeiraApp.Models.PessoaTipo", "PessoaTipo")
                        .WithMany()
                        .HasForeignKey("PessoaTipoId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ABrechozeiraApp.Models.PessoaStatus", "PessoaStatus")
                        .WithMany()
                        .HasForeignKey("StatusId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("PessoaCategoria");

                    b.Navigation("PessoaStatus");

                    b.Navigation("PessoaTipo");
                });

            modelBuilder.Entity("ABrechozeiraApp.Models.Produto", b =>
                {
                    b.HasOne("ABrechozeiraApp.Models.Grupo", "Grupo")
                        .WithMany()
                        .HasForeignKey("GrupoID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ABrechozeiraApp.Models.Marca", "Marca")
                        .WithMany()
                        .HasForeignKey("MarcaId");

                    b.HasOne("ABrechozeiraApp.Models.Pessoa", "PessoaPertence")
                        .WithMany()
                        .HasForeignKey("PessoaPertenceID");

                    b.HasOne("ABrechozeiraApp.Models.ProdutoStatus", "ProdutoStatus")
                        .WithMany()
                        .HasForeignKey("StatusId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ABrechozeiraApp.Models.Usuario", "UsuarioModificacao")
                        .WithMany()
                        .HasForeignKey("UsuarioModificacaoId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Grupo");

                    b.Navigation("Marca");

                    b.Navigation("PessoaPertence");

                    b.Navigation("ProdutoStatus");

                    b.Navigation("UsuarioModificacao");
                });

            modelBuilder.Entity("ABrechozeiraApp.Models.Usuario", b =>
                {
                    b.HasOne("ABrechozeiraApp.Models.NivelAcesso", "NivelAcesso")
                        .WithMany()
                        .HasForeignKey("NivelAcessoID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ABrechozeiraApp.Models.Pessoa", "Pessoa")
                        .WithMany()
                        .HasForeignKey("PessoaID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("NivelAcesso");

                    b.Navigation("Pessoa");
                });

            modelBuilder.Entity("ABrechozeiraApp.Models.Venda", b =>
                {
                    b.HasOne("ABrechozeiraApp.Models.Pessoa", "Cliente")
                        .WithMany()
                        .HasForeignKey("ClienteId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ABrechozeiraApp.Models.Live", "Live")
                        .WithMany()
                        .HasForeignKey("LiveId");

                    b.HasOne("ABrechozeiraApp.Models.Origem", "Origem")
                        .WithMany()
                        .HasForeignKey("OrigemID");

                    b.HasOne("ABrechozeiraApp.Models.Produto", "Produto")
                        .WithMany()
                        .HasForeignKey("ProdutoId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ABrechozeiraApp.Models.Usuario", "UsuarioModificacao")
                        .WithMany()
                        .HasForeignKey("UsuarioModificacaoId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Cliente");

                    b.Navigation("Live");

                    b.Navigation("Origem");

                    b.Navigation("Produto");

                    b.Navigation("UsuarioModificacao");
                });
#pragma warning restore 612, 618
        }
    }
}
