using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABrechozeiraApp.Migrations
{
    /// <inheritdoc />
    public partial class tb_live_add_tb_venda_fld_ordemlive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OrdemVendaLive",
                table: "Venda",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrdemVendaLive",
                table: "Venda");
        }
    }
}
