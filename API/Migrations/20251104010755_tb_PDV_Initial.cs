using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABrechozeiraApp.Migrations
{
    /// <inheritdoc />
    public partial class tb_PDV_Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Caixa",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    DataAbertura = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    SaldoInicial = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    DataFechamento = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    SaldoFechamento = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    Observacao = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Caixa", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "FormaPagamentoConfigPDV",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FormaPagamentoId = table.Column<int>(type: "int", nullable: false),
                    ExibirNoPDV = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    PermiteParcelamento = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    MaxParcelas = table.Column<int>(type: "int", nullable: true),
                    TaxaAdmPerc = table.Column<decimal>(type: "decimal(65,30)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormaPagamentoConfigPDV", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FormaPagamentoConfigPDV_FormaPagamento_FormaPagamentoId",
                        column: x => x.FormaPagamentoId,
                        principalTable: "FormaPagamento",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "VendaPdv",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Codigo = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    ClienteId = table.Column<int>(type: "int", nullable: true),
                    Status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DataVenda = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ValorBruto = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    Desconto = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    ValorLiquido = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    Observacao = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    UsuarioId = table.Column<int>(type: "int", nullable: true),
                    CaixaId = table.Column<int>(type: "int", nullable: true),
                    DataAlteracao = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VendaPdv", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VendaPdv_Pessoa_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Pessoa",
                        principalColumn: "Id");
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "CaixaMovimento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    CaixaId = table.Column<int>(type: "int", nullable: false),
                    Tipo = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Valor = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    ReferenciaId = table.Column<int>(type: "int", nullable: true),
                    Observacao = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DataRegistro = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CaixaMovimento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CaixaMovimento_Caixa_CaixaId",
                        column: x => x.CaixaId,
                        principalTable: "Caixa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "VendaPdvItem",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    VendaPdvId = table.Column<int>(type: "int", nullable: false),
                    ProdutoId = table.Column<int>(type: "int", nullable: true),
                    DescricaoItem = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Quantidade = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    PrecoUnitario = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    DescontoValor = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    DescontoPerc = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    Total = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    CodigoEstoque = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VendaPdvItem", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VendaPdvItem_Produto_ProdutoId",
                        column: x => x.ProdutoId,
                        principalTable: "Produto",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_VendaPdvItem_VendaPdv_VendaPdvId",
                        column: x => x.VendaPdvId,
                        principalTable: "VendaPdv",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "VendaPdvPagamento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    VendaPdvId = table.Column<int>(type: "int", nullable: false),
                    FormaPagamentoId = table.Column<int>(type: "int", nullable: false),
                    CondicaoPagamentoId = table.Column<int>(type: "int", nullable: true),
                    Valor = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    Observacao = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TransacaoId = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DataRegistro = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VendaPdvPagamento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VendaPdvPagamento_CondicaoPagamento_CondicaoPagamentoId",
                        column: x => x.CondicaoPagamentoId,
                        principalTable: "CondicaoPagamento",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_VendaPdvPagamento_FormaPagamento_FormaPagamentoId",
                        column: x => x.FormaPagamentoId,
                        principalTable: "FormaPagamento",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VendaPdvPagamento_VendaPdv_VendaPdvId",
                        column: x => x.VendaPdvId,
                        principalTable: "VendaPdv",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_CaixaMovimento_CaixaId",
                table: "CaixaMovimento",
                column: "CaixaId");

            migrationBuilder.CreateIndex(
                name: "IX_FormaPagamentoConfigPDV_FormaPagamentoId",
                table: "FormaPagamentoConfigPDV",
                column: "FormaPagamentoId");

            migrationBuilder.CreateIndex(
                name: "IX_VendaPdv_ClienteId",
                table: "VendaPdv",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_VendaPdvItem_ProdutoId",
                table: "VendaPdvItem",
                column: "ProdutoId");

            migrationBuilder.CreateIndex(
                name: "IX_VendaPdvItem_VendaPdvId",
                table: "VendaPdvItem",
                column: "VendaPdvId");

            migrationBuilder.CreateIndex(
                name: "IX_VendaPdvPagamento_CondicaoPagamentoId",
                table: "VendaPdvPagamento",
                column: "CondicaoPagamentoId");

            migrationBuilder.CreateIndex(
                name: "IX_VendaPdvPagamento_FormaPagamentoId",
                table: "VendaPdvPagamento",
                column: "FormaPagamentoId");

            migrationBuilder.CreateIndex(
                name: "IX_VendaPdvPagamento_VendaPdvId",
                table: "VendaPdvPagamento",
                column: "VendaPdvId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CaixaMovimento");

            migrationBuilder.DropTable(
                name: "FormaPagamentoConfigPDV");

            migrationBuilder.DropTable(
                name: "VendaPdvItem");

            migrationBuilder.DropTable(
                name: "VendaPdvPagamento");

            migrationBuilder.DropTable(
                name: "Caixa");

            migrationBuilder.DropTable(
                name: "VendaPdv");
        }
    }
}
