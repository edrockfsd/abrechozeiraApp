using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABrechozeiraApp.Migrations
{
    /// <inheritdoc />
    public partial class tb_Pedido_endereco_entrega_nullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pedido_CondicaoPagamento_CondicaoPagamentoID",
                table: "Pedido");

            migrationBuilder.DropForeignKey(
                name: "FK_Pedido_Endereco_EnderecoEntregaID",
                table: "Pedido");

            migrationBuilder.DropForeignKey(
                name: "FK_Pedido_FormaPagamento_FormaPagamentoID",
                table: "Pedido");

            migrationBuilder.AlterColumn<int>(
                name: "FormaPagamentoID",
                table: "Pedido",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "EnderecoEntregaID",
                table: "Pedido",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "CondicaoPagamentoID",
                table: "Pedido",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddForeignKey(
                name: "FK_Pedido_CondicaoPagamento_CondicaoPagamentoID",
                table: "Pedido",
                column: "CondicaoPagamentoID",
                principalTable: "CondicaoPagamento",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Pedido_Endereco_EnderecoEntregaID",
                table: "Pedido",
                column: "EnderecoEntregaID",
                principalTable: "Endereco",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Pedido_FormaPagamento_FormaPagamentoID",
                table: "Pedido",
                column: "FormaPagamentoID",
                principalTable: "FormaPagamento",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Pedido_CondicaoPagamento_CondicaoPagamentoID",
                table: "Pedido");

            migrationBuilder.DropForeignKey(
                name: "FK_Pedido_Endereco_EnderecoEntregaID",
                table: "Pedido");

            migrationBuilder.DropForeignKey(
                name: "FK_Pedido_FormaPagamento_FormaPagamentoID",
                table: "Pedido");

            migrationBuilder.AlterColumn<int>(
                name: "FormaPagamentoID",
                table: "Pedido",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "EnderecoEntregaID",
                table: "Pedido",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "CondicaoPagamentoID",
                table: "Pedido",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Pedido_CondicaoPagamento_CondicaoPagamentoID",
                table: "Pedido",
                column: "CondicaoPagamentoID",
                principalTable: "CondicaoPagamento",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Pedido_Endereco_EnderecoEntregaID",
                table: "Pedido",
                column: "EnderecoEntregaID",
                principalTable: "Endereco",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Pedido_FormaPagamento_FormaPagamentoID",
                table: "Pedido",
                column: "FormaPagamentoID",
                principalTable: "FormaPagamento",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
