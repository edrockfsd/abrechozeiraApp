using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABrechozeiraApp.Migrations
{
    /// <inheritdoc />
    public partial class pk_live_tb_venda : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Venda_LiveId",
                table: "Venda",
                column: "LiveId");

            migrationBuilder.AddForeignKey(
                name: "FK_Venda_Live_LiveId",
                table: "Venda",
                column: "LiveId",
                principalTable: "Live",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Venda_Live_LiveId",
                table: "Venda");

            migrationBuilder.DropIndex(
                name: "IX_Venda_LiveId",
                table: "Venda");
        }
    }
}
