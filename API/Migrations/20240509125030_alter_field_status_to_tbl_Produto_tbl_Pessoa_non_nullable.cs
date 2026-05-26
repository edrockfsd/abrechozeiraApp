using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABrechozeiraApp.Migrations
{
    /// <inheritdoc />
    public partial class alter_field_status_to_tbl_Produto_tbl_Pessoa_non_nullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pessoa_PessoaStatus_StatusId",
                table: "Pessoa");

            migrationBuilder.DropForeignKey(
                name: "FK_Produto_ProdutoStatus_StatusId",
                table: "Produto");

            migrationBuilder.AlterColumn<int>(
                name: "StatusId",
                table: "Produto",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "StatusId",
                table: "Pessoa",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Pessoa_PessoaStatus_StatusId",
                table: "Pessoa",
                column: "StatusId",
                principalTable: "PessoaStatus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Produto_ProdutoStatus_StatusId",
                table: "Produto",
                column: "StatusId",
                principalTable: "ProdutoStatus",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pessoa_PessoaStatus_StatusId",
                table: "Pessoa");

            migrationBuilder.DropForeignKey(
                name: "FK_Produto_ProdutoStatus_StatusId",
                table: "Produto");

            migrationBuilder.AlterColumn<int>(
                name: "StatusId",
                table: "Produto",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "StatusId",
                table: "Pessoa",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Pessoa_PessoaStatus_StatusId",
                table: "Pessoa",
                column: "StatusId",
                principalTable: "PessoaStatus",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Produto_ProdutoStatus_StatusId",
                table: "Produto",
                column: "StatusId",
                principalTable: "ProdutoStatus",
                principalColumn: "Id");
        }
    }
}
