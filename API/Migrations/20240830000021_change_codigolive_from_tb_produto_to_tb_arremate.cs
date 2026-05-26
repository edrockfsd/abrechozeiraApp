using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABrechozeiraApp.Migrations
{
    /// <inheritdoc />
    public partial class change_codigolive_from_tb_produto_to_tb_arremate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CodigoLive",
                table: "Produto");

            migrationBuilder.RenameColumn(
                name: "Sexto",
                table: "Produto",
                newName: "Sexo");

            migrationBuilder.AddColumn<int>(
                name: "CodigoLive",
                table: "Arremate",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CodigoLive",
                table: "Arremate");

            migrationBuilder.RenameColumn(
                name: "Sexo",
                table: "Produto",
                newName: "Sexto");

            migrationBuilder.AddColumn<int>(
                name: "CodigoLive",
                table: "Produto",
                type: "int",
                nullable: true);
        }
    }
}
