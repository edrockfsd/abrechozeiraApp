using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABrechozeiraApp.Migrations
{
    /// <inheritdoc />
    public partial class del_codigo_estoque_tb_prod_add_cod_estoque_to_tb_estoque : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CodigoEstoque",
                table: "Produto");

            migrationBuilder.AddColumn<int>(
                name: "CodigoEstoque",
                table: "Estoque",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CodigoEstoque",
                table: "Estoque");

            migrationBuilder.AddColumn<int>(
                name: "CodigoEstoque",
                table: "Produto",
                type: "int",
                nullable: true);
        }
    }
}
