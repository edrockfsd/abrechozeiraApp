using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABrechozeiraApp.Migrations
{
    /// <inheritdoc />
    public partial class tbl_produto_remover_pessoapertence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {


            migrationBuilder.AddForeignKey(
                name: "FK_Produto_PessoaGenero_GeneroID",
                table: "Produto",
                column: "GeneroID",
                principalTable: "PessoaGenero",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Produto_ProdutoPerfil_PerfilID",
                table: "Produto",
                column: "PerfilID",
                principalTable: "ProdutoPerfil",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
           

            

            migrationBuilder.AddColumn<int>(
                name: "PessoaPertenceID",
                table: "Produto",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Produto_PessoaPertenceID",
                table: "Produto",
                column: "PessoaPertenceID");

            migrationBuilder.AddForeignKey(
                name: "FK_Produto_PessoaGenero_Genero",
                table: "Produto",
                column: "Genero",
                principalTable: "PessoaGenero",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Produto_Pessoa_PessoaPertenceID",
                table: "Produto",
                column: "PessoaPertenceID",
                principalTable: "Pessoa",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Produto_ProdutoPerfil_Perfil",
                table: "Produto",
                column: "Perfil",
                principalTable: "ProdutoPerfil",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
