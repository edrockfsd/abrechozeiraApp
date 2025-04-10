using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABrechozeiraApp.Migrations
{
    /// <inheritdoc />
    public partial class tb_pessoa_add_obs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Observacoes",
                table: "Pessoa",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Observacoes",
                table: "Pessoa");
        }
    }
}
