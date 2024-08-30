using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABrechozeiraApp.Migrations
{
    /// <inheritdoc />
    public partial class tbl_produto_add_alter_usuModificacao_nonnullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Produto_Usuario_UsuarioModificacaoId",
                table: "Produto");

            migrationBuilder.AlterColumn<int>(
                name: "UsuarioModificacaoId",
                table: "Produto",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Produto_Usuario_UsuarioModificacaoId",
                table: "Produto",
                column: "UsuarioModificacaoId",
                principalTable: "Usuario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Produto_Usuario_UsuarioModificacaoId",
                table: "Produto");

            migrationBuilder.AlterColumn<int>(
                name: "UsuarioModificacaoId",
                table: "Produto",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Produto_Usuario_UsuarioModificacaoId",
                table: "Produto",
                column: "UsuarioModificacaoId",
                principalTable: "Usuario",
                principalColumn: "Id");
        }
    }
}
