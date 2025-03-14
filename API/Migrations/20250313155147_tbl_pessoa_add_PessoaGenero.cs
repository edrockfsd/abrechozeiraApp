using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABrechozeiraApp.Migrations
{
    /// <inheritdoc />
    public partial class tbl_pessoa_add_PessoaGenero : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PessoaGeneroId",
                table: "Pessoa",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pessoa_PessoaGeneroId",
                table: "Pessoa",
                column: "PessoaGeneroId");

            migrationBuilder.AddForeignKey(
                name: "FK_Pessoa_PessoaGenero_PessoaGeneroId",
                table: "Pessoa",
                column: "PessoaGeneroId",
                principalTable: "PessoaGenero",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pessoa_PessoaGenero_PessoaGeneroId",
                table: "Pessoa");

            migrationBuilder.DropIndex(
                name: "IX_Pessoa_PessoaGeneroId",
                table: "Pessoa");

            migrationBuilder.DropColumn(
                name: "PessoaGeneroId",
                table: "Pessoa");
        }
    }
}
