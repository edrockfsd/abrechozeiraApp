using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABrechozeiraApp.Migrations
{
    /// <inheritdoc />
    public partial class tbl_produtos_alt_genero_perfil_names : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "Perfil",
                table: "Produto",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "varchar(1)",
                oldNullable: true)
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<int>(
                name: "Genero",
                table: "Produto",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "varchar(1)",
                oldNullable: true)
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Produto_Genero",
                table: "Produto",
                column: "Genero");

            migrationBuilder.CreateIndex(
                name: "IX_Produto_Perfil",
                table: "Produto",
                column: "Perfil");

            migrationBuilder.AddForeignKey(
                name: "FK_Produto_PessoaGenero_Genero",
                table: "Produto",
                column: "Genero",
                principalTable: "PessoaGenero",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Produto_ProdutoPerfil_Perfil",
                table: "Produto",
                column: "Perfil",
                principalTable: "ProdutoPerfil",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Produto_PessoaGenero_Genero",
                table: "Produto");

            migrationBuilder.DropForeignKey(
                name: "FK_Produto_ProdutoPerfil_Perfil",
                table: "Produto");

            migrationBuilder.DropIndex(
                name: "IX_Produto_Genero",
                table: "Produto");

            migrationBuilder.DropIndex(
                name: "IX_Produto_Perfil",
                table: "Produto");

            migrationBuilder.AlterColumn<string>(
                name: "Perfil",
                table: "Produto",
                type: "varchar(1)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Genero",
                table: "Produto",
                type: "varchar(1)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int")
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
