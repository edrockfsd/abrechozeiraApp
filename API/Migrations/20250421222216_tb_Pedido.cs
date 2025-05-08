using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABrechozeiraApp.Migrations
{
    /// <inheritdoc />
    public partial class tb_Pedido : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Pedido",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    PedidoCodigo = table.Column<int>(type: "int", nullable: false),
                    DataLancamento = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ClienteID = table.Column<int>(type: "int", nullable: false),
                    DescontoPorcentagem = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    ValorFrete = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    PedidoStatusID = table.Column<int>(type: "int", nullable: false),
                    ValorTotal = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    CondicaoPagamentoID = table.Column<int>(type: "int", nullable: false),
                    FormaPagamentoID = table.Column<int>(type: "int", nullable: false),
                    EnderecoEntregaID = table.Column<int>(type: "int", nullable: false),
                    Observacoes = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DataAlteracao = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UsuarioModificacaoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pedido", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pedido_CondicaoPagamento_CondicaoPagamentoID",
                        column: x => x.CondicaoPagamentoID,
                        principalTable: "CondicaoPagamento",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Pedido_Endereco_EnderecoEntregaID",
                        column: x => x.EnderecoEntregaID,
                        principalTable: "Endereco",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Pedido_FormaPagamento_FormaPagamentoID",
                        column: x => x.FormaPagamentoID,
                        principalTable: "FormaPagamento",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Pedido_PedidoStatus_PedidoStatusID",
                        column: x => x.PedidoStatusID,
                        principalTable: "PedidoStatus",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Pedido_Pessoa_ClienteID",
                        column: x => x.ClienteID,
                        principalTable: "Pessoa",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Pedido_Usuario_UsuarioModificacaoId",
                        column: x => x.UsuarioModificacaoId,
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Pedido_ClienteID",
                table: "Pedido",
                column: "ClienteID");

            migrationBuilder.CreateIndex(
                name: "IX_Pedido_CondicaoPagamentoID",
                table: "Pedido",
                column: "CondicaoPagamentoID");

            migrationBuilder.CreateIndex(
                name: "IX_Pedido_EnderecoEntregaID",
                table: "Pedido",
                column: "EnderecoEntregaID");

            migrationBuilder.CreateIndex(
                name: "IX_Pedido_FormaPagamentoID",
                table: "Pedido",
                column: "FormaPagamentoID");

            migrationBuilder.CreateIndex(
                name: "IX_Pedido_PedidoStatusID",
                table: "Pedido",
                column: "PedidoStatusID");

            migrationBuilder.CreateIndex(
                name: "IX_Pedido_UsuarioModificacaoId",
                table: "Pedido",
                column: "UsuarioModificacaoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Pedido");
        }
    }
}
