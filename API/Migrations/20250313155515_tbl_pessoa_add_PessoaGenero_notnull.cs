using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABrechozeiraApp.Migrations
{
    /// <inheritdoc />
    public partial class tbl_pessoa_add_PessoaGenero_notnull : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pessoa_PessoaGenero_PessoaGeneroId",
                table: "Pessoa");

            migrationBuilder.AlterColumn<int>(
                name: "PessoaGeneroId",
                table: "Pessoa",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Pessoa_PessoaGenero_PessoaGeneroId",
                table: "Pessoa",
                column: "PessoaGeneroId",
                principalTable: "PessoaGenero",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pessoa_PessoaGenero_PessoaGeneroId",
                table: "Pessoa");

            migrationBuilder.AlterColumn<int>(
                name: "PessoaGeneroId",
                table: "Pessoa",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Pessoa_PessoaGenero_PessoaGeneroId",
                table: "Pessoa",
                column: "PessoaGeneroId",
                principalTable: "PessoaGenero",
                principalColumn: "Id");
        }
    }
}
