using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABrechozeiraApp.Migrations
{
    /// <inheritdoc />
    public partial class field_dtvenda_dtpgt_dtalt_usumod_tbl_venda_v2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DataAlteracao",
                table: "Venda",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataPagamento",
                table: "Venda",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataVenda",
                table: "Venda",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UsuarioModificacaoId",
                table: "Venda",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Venda_UsuarioModificacaoId",
                table: "Venda",
                column: "UsuarioModificacaoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Venda_Usuario_UsuarioModificacaoId",
                table: "Venda",
                column: "UsuarioModificacaoId",
                principalTable: "Usuario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Venda_Usuario_UsuarioModificacaoId",
                table: "Venda");

            migrationBuilder.DropIndex(
                name: "IX_Venda_UsuarioModificacaoId",
                table: "Venda");

            migrationBuilder.DropColumn(
                name: "DataAlteracao",
                table: "Venda");

            migrationBuilder.DropColumn(
                name: "DataPagamento",
                table: "Venda");

            migrationBuilder.DropColumn(
                name: "DataVenda",
                table: "Venda");

            migrationBuilder.DropColumn(
                name: "UsuarioModificacaoId",
                table: "Venda");
        }
    }
}
