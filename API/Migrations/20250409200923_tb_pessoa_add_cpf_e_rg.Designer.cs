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
    [Migration("20250409200923_tb_pessoa_add_cpf_e_rg")]
    partial class tb_pessoa_add_cpf_e_rg
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

                    b.Property<int?>("CodigoLive")
                        .HasColumnType("int");

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

            modelBuilder.Entity("ABrechozeiraApp.Models.Endereco", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Bairro")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.Property<string>("CEP")
                        .HasMaxLength(8)
                        .HasColumnType("varchar(8)");

                    b.Property<int>("CodigoLocalidadeIBGE")
                        .HasColumnType("int");

                    b.Property<string>("Complemento")
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.Property<DateTime?>("DataAlteracao")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Estado")
                        .IsRequired()
                        .HasMaxLength(30)
                        .HasColumnType("varchar(30)");

                    b.Property<string>("Localidade")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.Property<string>("Logradouro")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");

                    b.Property<int>("PessoaID")
                        .HasColumnType("int");

                    b.Property<int>("TipoEnderecoId")
                        .HasColumnType("int");

                    b.Property<string>("Unidade")
                        .IsRequired()
                        .HasMaxLength(8)
                        .HasColumnType("varchar(8)");

                    b.Property<int>("UsuarioModificacaoId")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("PessoaID");

                    b.HasIndex("TipoEnderecoId");

                    b.HasIndex("UsuarioModificacaoId");

                    b.ToTable("Endereco");
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

                    b.Property<string>("CPF")
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.Property<DateTime?>("DataInclusao")
                        .HasColumnType("datetime(6)");

                    b.Property<DateTime?>("DataNascimento")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Email")
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");

                    b.Property<string>("NickName")
                        .HasColumnType("longtext");

                    b.Property<string>("Nome")
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.Property<int>("PessoaCategoriaId")
                        .HasColumnType("int");

                    b.Property<int>("PessoaGeneroId")
                        .HasColumnType("int");

                    b.Property<int>("PessoaTipoId")
                        .HasColumnType("int");

                    b.Property<string>("RG")
                        .HasColumnType("longtext");

                    b.Property<int>("StatusId")
                        .HasColumnType("int");

                    b.Property<string>("Telefone")
                        .HasMaxLength(13)
                        .HasColumnType("varchar(13)");

                    b.HasKey("Id");

                    b.HasIndex("PessoaCategoriaId");

                    b.HasIndex("PessoaGeneroId");

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

            modelBuilder.Entity("ABrechozeiraApp.Models.PessoaGenero", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    MySqlPropertyBuilderExtensions.UseMySqlIdentityColumn(b.Property<int>("Id"));

                    b.Property<string>("Descricao")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.Property<string>("Sigla")
                        .IsRequired()
                        .HasMaxLength(1)
                        .HasColumnType("varchar(1)");

                    b.HasKey("Id");

                    b.ToTable("PessoaGenero");
                });

            modelBuilder.Entity("ABrechozeiraApp.Models.PessoaPerfil", b =>
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

                    b.ToTable("PessoaPerfil");
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

                    b.Property<DateTime?>("DataAlteracao")
                        .HasColumnType("datetime(6)");

                    b.Property<DateTime?>("DataCompra")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Descricao")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");

                    b.Property<int>("GeneroID")
                        .HasColumnType("int");

                    b.Property<int>("GrupoID")
                        .HasColumnType("int");

                    b.Property<int?>("MarcaId")
                        .HasColumnType("int");

                    b.Property<string>("Origem")
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.Property<int>("PerfilID")
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

                    b.HasIndex("GeneroID");

                    b.HasIndex("GrupoID");

                    b.HasIndex("MarcaId");

                    b.HasIndex("PerfilID");

                    b.HasIndex("StatusId");

                    b.HasIndex("UsuarioModificacaoId");

                    b.ToTable("Produto");
                });

            modelBuilder.Entity("ABrechozeiraApp.Models.ProdutoGrupo", b =>
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

                    b.ToTable("ProdutoGrupo");
                });

            modelBuilder.Entity("ABrechozeiraApp.Models.ProdutoMarca", b =>
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

                    b.ToTable("ProdutoMarca");
                });

            modelBuilder.Entity("ABrechozeiraApp.Models.ProdutoPerfil", b =>
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

                    b.ToTable("ProdutoPerfil");
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

            modelBuilder.Entity("ABrechozeiraApp.Models.TipoEndereco", b =>
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

                    b.ToTable("TipoEndereco");
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

            modelBuilder.Entity("ABrechozeiraApp.Models.Endereco", b =>
                {
                    b.HasOne("ABrechozeiraApp.Models.Pessoa", "Pessoa")
                        .WithMany()
                        .HasForeignKey("PessoaID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ABrechozeiraApp.Models.TipoEndereco", "TipoEndereco")
                        .WithMany()
                        .HasForeignKey("TipoEnderecoId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ABrechozeiraApp.Models.Usuario", "UsuarioModificacao")
                        .WithMany()
                        .HasForeignKey("UsuarioModificacaoId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Pessoa");

                    b.Navigation("TipoEndereco");

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

                    b.HasOne("ABrechozeiraApp.Models.PessoaGenero", "PessoaGenero")
                        .WithMany()
                        .HasForeignKey("PessoaGeneroId")
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

                    b.Navigation("PessoaGenero");

                    b.Navigation("PessoaStatus");

                    b.Navigation("PessoaTipo");
                });

            modelBuilder.Entity("ABrechozeiraApp.Models.Produto", b =>
                {
                    b.HasOne("ABrechozeiraApp.Models.PessoaGenero", "PessoaGenero")
                        .WithMany()
                        .HasForeignKey("GeneroID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ABrechozeiraApp.Models.ProdutoGrupo", "ProdutoGrupo")
                        .WithMany()
                        .HasForeignKey("GrupoID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("ABrechozeiraApp.Models.ProdutoMarca", "Marca")
                        .WithMany()
                        .HasForeignKey("MarcaId");

                    b.HasOne("ABrechozeiraApp.Models.ProdutoPerfil", "ProdutoPerfil")
                        .WithMany()
                        .HasForeignKey("PerfilID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

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

                    b.Navigation("Marca");

                    b.Navigation("PessoaGenero");

                    b.Navigation("ProdutoGrupo");

                    b.Navigation("ProdutoPerfil");

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
