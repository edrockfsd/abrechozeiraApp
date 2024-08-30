using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABrechozeiraApp.Migrations
{
    /// <inheritdoc />
    public partial class tbl_arremate_add_liveid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LiveId",
                table: "Arremate",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Arremate_LiveId",
                table: "Arremate",
                column: "LiveId");

            migrationBuilder.AddForeignKey(
                name: "FK_Arremate_Live_LiveId",
                table: "Arremate",
                column: "LiveId",
                principalTable: "Live",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Arremate_Live_LiveId",
                table: "Arremate");

            migrationBuilder.DropIndex(
                name: "IX_Arremate_LiveId",
                table: "Arremate");

            migrationBuilder.DropColumn(
                name: "LiveId",
                table: "Arremate");
        }
    }
}
