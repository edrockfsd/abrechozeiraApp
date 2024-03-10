using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABrechozeiraApp.Migrations
{
    /// <inheritdoc />
    public partial class change_tb_vendaV4 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OrigemID",
                table: "Venda",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Origem",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Descricao = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Origem", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Venda_OrigemID",
                table: "Venda",
                column: "OrigemID");

            migrationBuilder.AddForeignKey(
                name: "FK_Venda_Origem_OrigemID",
                table: "Venda",
                column: "OrigemID",
                principalTable: "Origem",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Venda_Origem_OrigemID",
                table: "Venda");

            migrationBuilder.DropTable(
                name: "Origem");

            migrationBuilder.DropIndex(
                name: "IX_Venda_OrigemID",
                table: "Venda");

            migrationBuilder.DropColumn(
                name: "OrigemID",
                table: "Venda");
        }
    }
}
