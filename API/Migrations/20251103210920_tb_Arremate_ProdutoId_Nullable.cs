using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABrechozeiraApp.Migrations
{
    /// <inheritdoc />
    public partial class tb_Arremate_ProdutoId_Nullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
           


            migrationBuilder.AlterColumn<int>(
                name: "ProdutoId",
                table: "Arremate",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");


            migrationBuilder.AddForeignKey(
                name: "FK_Arremate_Produto_ProdutoId",
                table: "Arremate",
                column: "ProdutoId",
                principalTable: "Produto",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Arremate_Produto_ProdutoId",
                table: "Arremate");

            migrationBuilder.DropIndex(
                name: "IX_User_PessoaId",
                table: "User");

            migrationBuilder.AlterColumn<int>(
                name: "ProdutoId",
                table: "Arremate",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_User_PessoaId",
                table: "User",
                column: "PessoaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Arremate_Produto_ProdutoId",
                table: "Arremate",
                column: "ProdutoId",
                principalTable: "Produto",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
