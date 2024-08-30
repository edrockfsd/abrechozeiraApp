using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABrechozeiraApp.Migrations
{
    /// <inheritdoc />
    public partial class add_pessoapertence_datacompra_tb_produto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DataCompra",
                table: "Produto",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PessoaPertenceID",
                table: "Produto",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Produto_PessoaPertenceID",
                table: "Produto",
                column: "PessoaPertenceID");

            migrationBuilder.AddForeignKey(
                name: "FK_Produto_Pessoa_PessoaPertenceID",
                table: "Produto",
                column: "PessoaPertenceID",
                principalTable: "Pessoa",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Produto_Pessoa_PessoaPertenceID",
                table: "Produto");

            migrationBuilder.DropIndex(
                name: "IX_Produto_PessoaPertenceID",
                table: "Produto");

            migrationBuilder.DropColumn(
                name: "DataCompra",
                table: "Produto");

            migrationBuilder.DropColumn(
                name: "PessoaPertenceID",
                table: "Produto");
        }
    }
}
