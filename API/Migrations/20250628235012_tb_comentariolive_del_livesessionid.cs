using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABrechozeiraApp.Migrations
{
    /// <inheritdoc />
    public partial class tb_comentariolive_del_livesessionid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ComentarioLive_LiveSession_LiveSessionId",
                table: "ComentarioLive");

            migrationBuilder.DropIndex(
                name: "IX_ComentarioLive_LiveSessionId",
                table: "ComentarioLive");

            migrationBuilder.DropColumn(
                name: "LiveSessionId",
                table: "ComentarioLive");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LiveSessionId",
                table: "ComentarioLive",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_ComentarioLive_LiveSessionId",
                table: "ComentarioLive",
                column: "LiveSessionId");

            migrationBuilder.AddForeignKey(
                name: "FK_ComentarioLive_LiveSession_LiveSessionId",
                table: "ComentarioLive",
                column: "LiveSessionId",
                principalTable: "LiveSession",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
