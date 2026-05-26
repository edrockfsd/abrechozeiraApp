using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABrechozeiraApp.Migrations
{
    /// <inheritdoc />
    public partial class tbUser_PessoaId_nonNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_User_Pessoa_PessoaID",
                table: "User");

            migrationBuilder.RenameColumn(
                name: "PessoaID",
                table: "User",
                newName: "PessoaId");

            migrationBuilder.RenameIndex(
                name: "IX_User_PessoaID",
                table: "User",
                newName: "IX_User_PessoaId");

            migrationBuilder.AlterColumn<int>(
                name: "PessoaId",
                table: "User",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_User_Pessoa_PessoaId",
                table: "User",
                column: "PessoaId",
                principalTable: "Pessoa",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_User_Pessoa_PessoaId",
                table: "User");

            migrationBuilder.RenameColumn(
                name: "PessoaId",
                table: "User",
                newName: "PessoaID");

            migrationBuilder.RenameIndex(
                name: "IX_User_PessoaId",
                table: "User",
                newName: "IX_User_PessoaID");

            migrationBuilder.AlterColumn<int>(
                name: "PessoaID",
                table: "User",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_User_Pessoa_PessoaID",
                table: "User",
                column: "PessoaID",
                principalTable: "Pessoa",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
