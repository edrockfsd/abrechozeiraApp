using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABrechozeiraApp.Migrations
{
    /// <inheritdoc />
    public partial class change_codigolive_from_produto_to_venda : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CodigoLive",
                table: "Produto");

            migrationBuilder.AddColumn<int>(
                name: "CodigoLive",
                table: "Venda",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CodigoLive",
                table: "Venda");

            migrationBuilder.AddColumn<int>(
                name: "CodigoLive",
                table: "Produto",
                type: "int",
                nullable: true);
        }
    }
}
