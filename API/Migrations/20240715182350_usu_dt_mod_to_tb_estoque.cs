using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABrechozeiraApp.Migrations
{
    /// <inheritdoc />
    public partial class usu_dt_mod_to_tb_estoque : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DataAlteracao",
                table: "Estoque",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UsuarioModificacaoId",
                table: "Estoque",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Estoque_UsuarioModificacaoId",
                table: "Estoque",
                column: "UsuarioModificacaoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Estoque_Usuario_UsuarioModificacaoId",
                table: "Estoque",
                column: "UsuarioModificacaoId",
                principalTable: "Usuario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Estoque_Usuario_UsuarioModificacaoId",
                table: "Estoque");

            migrationBuilder.DropIndex(
                name: "IX_Estoque_UsuarioModificacaoId",
                table: "Estoque");

            migrationBuilder.DropColumn(
                name: "DataAlteracao",
                table: "Estoque");

            migrationBuilder.DropColumn(
                name: "UsuarioModificacaoId",
                table: "Estoque");
        }
    }
}
