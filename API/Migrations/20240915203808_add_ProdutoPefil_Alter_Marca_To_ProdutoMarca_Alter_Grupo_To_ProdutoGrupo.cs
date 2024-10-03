using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABrechozeiraApp.Migrations
{
    /// <inheritdoc />
    public partial class add_ProdutoPefil_Alter_Marca_To_ProdutoMarca_Alter_Grupo_To_ProdutoGrupo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Produto_Grupo_GrupoID",
                table: "Produto");

            migrationBuilder.DropForeignKey(
                name: "FK_Produto_Marca_MarcaId",
                table: "Produto");

            migrationBuilder.DropTable(
                name: "Grupo");

            migrationBuilder.DropIndex(
                name: "IX_Produto_GrupoID",
                table: "Produto");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Marca",
                table: "Marca");

            migrationBuilder.RenameTable(
                name: "Marca",
                newName: "ProdutoMarca");

            migrationBuilder.AddColumn<int>(
                name: "ProdutoGrupoId",
                table: "Produto",
                type: "int",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_ProdutoMarca",
                table: "ProdutoMarca",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "ProdutoGrupo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Descricao = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProdutoGrupo", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

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

            migrationBuilder.AddForeignKey(
                name: "FK_Produto_ProdutoMarca_MarcaId",
                table: "Produto",
                column: "MarcaId",
                principalTable: "ProdutoMarca",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Produto_ProdutoGrupo_ProdutoGrupoId",
                table: "Produto");

            migrationBuilder.DropForeignKey(
                name: "FK_Produto_ProdutoMarca_MarcaId",
                table: "Produto");

            migrationBuilder.DropTable(
                name: "ProdutoGrupo");

            migrationBuilder.DropIndex(
                name: "IX_Produto_ProdutoGrupoId",
                table: "Produto");

            migrationBuilder.DropPrimaryKey(
                name: "PK_ProdutoMarca",
                table: "ProdutoMarca");

            migrationBuilder.DropColumn(
                name: "ProdutoGrupoId",
                table: "Produto");

            migrationBuilder.RenameTable(
                name: "ProdutoMarca",
                newName: "Marca");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Marca",
                table: "Marca",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Grupo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Descricao = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Grupo", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Produto_GrupoID",
                table: "Produto",
                column: "GrupoID");

            migrationBuilder.AddForeignKey(
                name: "FK_Produto_Grupo_GrupoID",
                table: "Produto",
                column: "GrupoID",
                principalTable: "Grupo",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Produto_Marca_MarcaId",
                table: "Produto",
                column: "MarcaId",
                principalTable: "Marca",
                principalColumn: "Id");
        }
    }
}
