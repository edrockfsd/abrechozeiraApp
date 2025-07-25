using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABrechozeiraApp.Migrations
{
    /// <inheritdoc />
    public partial class tb_live_session_alt_comentario_live : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LiveSessionId",
                table: "ComentarioLive",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "LiveSession",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    LiveVideoId = table.Column<long>(type: "bigint", nullable: false),
                    Status = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    StartedAt = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    EndedAt = table.Column<DateTime>(type: "datetime(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LiveSession", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ComentarioLive_LiveSession_LiveSessionId",
                table: "ComentarioLive");

            migrationBuilder.DropTable(
                name: "LiveSession");

            migrationBuilder.DropIndex(
                name: "IX_ComentarioLive_LiveSessionId",
                table: "ComentarioLive");

            migrationBuilder.DropColumn(
                name: "LiveSessionId",
                table: "ComentarioLive");
        }
    }
}
