﻿// <auto-generated />
using System;
using ABrechozeiraApp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace ABrechozeiraApp.Migrations
{
    [DbContext(typeof(AbrechozeiraContext))]
    [Migration("20240120142739_inicial08")]
    partial class inicial08
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.1")
                .HasAnnotation("Relational:MaxIdentifierLength", 64);

            modelBuilder.Entity("ABrechozeiraApp.Models.NivelAcesso", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Descricao")
                        .IsRequired()
                        .HasMaxLength(200)
                        .HasColumnType("varchar(200)");

                    b.HasKey("Id");

                    b.ToTable("NivelAcesso");
                });

            modelBuilder.Entity("ABrechozeiraApp.Models.Pessoa", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<DateTime?>("DataNascimento")
                        .HasColumnType("datetime(6)");

                    b.Property<string>("Email")
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");

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

                    b.Property<string>("Telefone")
                        .HasMaxLength(13)
                        .HasColumnType("varchar(13)");

                    b.HasKey("Id");

                    b.HasIndex("PessoaCategoriaId");

                    b.HasIndex("PessoaTipoId");

                    b.ToTable("Pessoa");
                });

            modelBuilder.Entity("ABrechozeiraApp.Models.PessoaCategoria", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Descricao")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.HasKey("Id");

                    b.ToTable("PessoaCategoria");
                });

            modelBuilder.Entity("ABrechozeiraApp.Models.PessoaTipo", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

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

                    b.Property<int?>("CodigoEstoque")
                        .HasColumnType("int");

                    b.Property<int?>("CodigoLive")
                        .HasColumnType("int");

                    b.Property<string>("Descricao")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("varchar(100)");

                    b.Property<string>("Grupo")
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.Property<string>("Origem")
                        .HasColumnType("varchar(1)");

                    b.Property<decimal?>("PrecoCusto")
                        .HasColumnType("decimal(65,30)");

                    b.Property<decimal?>("PrecoVenda")
                        .HasColumnType("decimal(65,30)");

                    b.Property<string>("Tamanho")
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.HasKey("Id");

                    b.ToTable("Produto");
                });

            modelBuilder.Entity("ABrechozeiraApp.Models.Status", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    b.Property<string>("Descricao")
                        .IsRequired()
                        .HasMaxLength(50)
                        .HasColumnType("varchar(50)");

                    b.HasKey("Id");

                    b.ToTable("Status");
                });

            modelBuilder.Entity("ABrechozeiraApp.Models.Usuario", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

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

                    b.Property<int>("ClienteId")
                        .HasColumnType("int");

                    b.Property<decimal?>("Desconto")
                        .HasColumnType("decimal(65,30)");

                    b.Property<decimal>("PrecoVenda")
                        .HasColumnType("decimal(65,30)");

                    b.Property<int>("ProdutoId")
                        .HasColumnType("int");

                    b.Property<int>("Quantidade")
                        .HasColumnType("int");

                    b.HasKey("Id");

                    b.HasIndex("ClienteId");

                    b.HasIndex("ProdutoId");

                    b.ToTable("Venda");
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

                    b.Navigation("PessoaCategoria");

                    b.Navigation("PessoaTipo");
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

                    b.HasOne("ABrechozeiraApp.Models.Produto", "Produto")
                        .WithMany()
                        .HasForeignKey("ProdutoId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Cliente");

                    b.Navigation("Produto");
                });
#pragma warning restore 612, 618
        }
    }
}
