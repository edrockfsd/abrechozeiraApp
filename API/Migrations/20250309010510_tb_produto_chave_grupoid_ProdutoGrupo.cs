using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABrechozeiraApp.Migrations
{
    /// <inheritdoc />
    public partial class tb_produto_chave_grupoid_ProdutoGrupo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Produto_ProdutoGrupo_ProdutoGrupoId",
                table: "Produto");

            migrationBuilder.DropIndex(
                name: "IX_Produto_ProdutoGrupoId",
                table: "Produto");

            migrationBuilder.DropColumn(
                name: "ProdutoGrupoId",
                table: "Produto");

            migrationBuilder.CreateIndex(
                name: "IX_Produto_GrupoID",
                table: "Produto",
                column: "GrupoID");

            migrationBuilder.AddForeignKey(
                name: "FK_Produto_ProdutoGrupo_GrupoID",
                table: "Produto",
                column: "GrupoID",
                principalTable: "ProdutoGrupo",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Produto_ProdutoGrupo_GrupoID",
                table: "Produto");

            migrationBuilder.DropIndex(
                name: "IX_Produto_GrupoID",
                table: "Produto");

            migrationBuilder.AddColumn<int>(
                name: "ProdutoGrupoId",
                table: "Produto",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Produto_ProdutoGrupoId",
                table: "Produto",
                column: "ProdutoGrupoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Produto_ProdutoGrupo_ProdutoGrupoId",
                table: "Produto",
                column: "ProdutoGrupoId",
                principalTable: "ProdutoGrupo",
                principalColumn: "Id");
        }
    }
}
