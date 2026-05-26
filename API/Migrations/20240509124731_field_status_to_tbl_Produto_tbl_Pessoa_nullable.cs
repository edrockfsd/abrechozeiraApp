using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABrechozeiraApp.Migrations
{
    /// <inheritdoc />
    public partial class field_status_to_tbl_Produto_tbl_Pessoa_nullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "StatusId",
                table: "Produto",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "StatusId",
                table: "Pessoa",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Produto_StatusId",
                table: "Produto",
                column: "StatusId");

            migrationBuilder.CreateIndex(
                name: "IX_Pessoa_StatusId",
                table: "Pessoa",
                column: "StatusId");

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pessoa_PessoaStatus_StatusId",
                table: "Pessoa");

            migrationBuilder.DropForeignKey(
                name: "FK_Produto_ProdutoStatus_StatusId",
                table: "Produto");

            migrationBuilder.DropIndex(
                name: "IX_Produto_StatusId",
                table: "Produto");

            migrationBuilder.DropIndex(
                name: "IX_Pessoa_StatusId",
                table: "Pessoa");

            migrationBuilder.DropColumn(
                name: "StatusId",
                table: "Produto");

            migrationBuilder.DropColumn(
                name: "StatusId",
                table: "Pessoa");
        }
    }
}
