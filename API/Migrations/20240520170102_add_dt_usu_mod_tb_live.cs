using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABrechozeiraApp.Migrations
{
    /// <inheritdoc />
    public partial class add_dt_usu_mod_tb_live : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DataAlteracao",
                table: "Live",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UsuarioModificacaoId",
                table: "Live",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Live_UsuarioModificacaoId",
                table: "Live",
                column: "UsuarioModificacaoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Live_Usuario_UsuarioModificacaoId",
                table: "Live",
                column: "UsuarioModificacaoId",
                principalTable: "Usuario",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Live_Usuario_UsuarioModificacaoId",
                table: "Live");

            migrationBuilder.DropIndex(
                name: "IX_Live_UsuarioModificacaoId",
                table: "Live");

            migrationBuilder.DropColumn(
                name: "DataAlteracao",
                table: "Live");

            migrationBuilder.DropColumn(
                name: "UsuarioModificacaoId",
                table: "Live");
        }
    }
}
