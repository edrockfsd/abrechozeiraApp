using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABrechozeiraApp.Migrations
{
    /// <inheritdoc />
    public partial class remove_null_usu_mod_tbl_live : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Live_Usuario_UsuarioModificacaoId",
                table: "Live");

            migrationBuilder.AlterColumn<int>(
                name: "UsuarioModificacaoId",
                table: "Live",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Live_Usuario_UsuarioModificacaoId",
                table: "Live",
                column: "UsuarioModificacaoId",
                principalTable: "Usuario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Live_Usuario_UsuarioModificacaoId",
                table: "Live");

            migrationBuilder.AlterColumn<int>(
                name: "UsuarioModificacaoId",
                table: "Live",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Live_Usuario_UsuarioModificacaoId",
                table: "Live",
                column: "UsuarioModificacaoId",
                principalTable: "Usuario",
                principalColumn: "Id");
        }
    }
}
