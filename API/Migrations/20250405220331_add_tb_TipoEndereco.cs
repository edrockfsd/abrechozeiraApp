using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABrechozeiraApp.Migrations
{
    /// <inheritdoc />
    public partial class add_tb_TipoEndereco : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TipoEnderecoID",
                table: "Endereco",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "TipoEndereco",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Descricao = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TipoEndereco", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Endereco_TipoEnderecoID",
                table: "Endereco",
                column: "TipoEnderecoID");

            migrationBuilder.AddForeignKey(
                name: "FK_Endereco_TipoEndereco_TipoEnderecoID",
                table: "Endereco",
                column: "TipoEnderecoID",
                principalTable: "TipoEndereco",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Endereco_TipoEndereco_TipoEnderecoID",
                table: "Endereco");

            migrationBuilder.DropTable(
                name: "TipoEndereco");

            migrationBuilder.DropIndex(
                name: "IX_Endereco_TipoEnderecoID",
                table: "Endereco");

            migrationBuilder.DropColumn(
                name: "TipoEnderecoID",
                table: "Endereco");
        }
    }
}
