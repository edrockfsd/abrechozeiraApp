using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABrechozeiraApp.Migrations
{
    /// <inheritdoc />
    public partial class change_tbl_produto_sexo_to_genero : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Sexo",
                table: "Produto",
                newName: "Genero");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Genero",
                table: "Produto",
                newName: "Sexo");
        }
    }
}
