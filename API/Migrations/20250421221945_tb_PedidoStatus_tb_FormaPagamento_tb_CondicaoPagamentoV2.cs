using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABrechozeiraApp.Migrations
{
    /// <inheritdoc />
    public partial class tb_PedidoStatus_tb_FormaPagamento_tb_CondicaoPagamentoV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CondicaoPagamento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Descricao = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DataAlteracao = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UsuarioModificacaoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CondicaoPagamento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CondicaoPagamento_Usuario_UsuarioModificacaoId",
                        column: x => x.UsuarioModificacaoId,
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "FormaPagamento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Descricao = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DataAlteracao = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UsuarioModificacaoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormaPagamento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FormaPagamento_Usuario_UsuarioModificacaoId",
                        column: x => x.UsuarioModificacaoId,
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "PedidoStatus",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Descricao = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    DataAlteracao = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    UsuarioModificacaoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PedidoStatus", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PedidoStatus_Usuario_UsuarioModificacaoId",
                        column: x => x.UsuarioModificacaoId,
                        principalTable: "Usuario",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_CondicaoPagamento_UsuarioModificacaoId",
                table: "CondicaoPagamento",
                column: "UsuarioModificacaoId");

            migrationBuilder.CreateIndex(
                name: "IX_FormaPagamento_UsuarioModificacaoId",
                table: "FormaPagamento",
                column: "UsuarioModificacaoId");

            migrationBuilder.CreateIndex(
                name: "IX_PedidoStatus_UsuarioModificacaoId",
                table: "PedidoStatus",
                column: "UsuarioModificacaoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CondicaoPagamento");

            migrationBuilder.DropTable(
                name: "FormaPagamento");

            migrationBuilder.DropTable(
                name: "PedidoStatus");
        }
    }
}
