using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABrechozeiraApp.Migrations
{
    /// <inheritdoc />
    public partial class AlterarVendaFluxoPedido_NfceVendaId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Venda_Produto_ProdutoId",
                table: "Venda");

            migrationBuilder.DropIndex(
                name: "IX_Venda_ProdutoId",
                table: "Venda");

            migrationBuilder.DropColumn(
                name: "ProdutoId",
                table: "Venda");

            migrationBuilder.RenameColumn(
                name: "ValorVenda",
                table: "Venda",
                newName: "ValorTotal");

            migrationBuilder.RenameColumn(
                name: "Quantidade",
                table: "Venda",
                newName: "PedidoId");

            migrationBuilder.RenameColumn(
                name: "OrdemVendaLive",
                table: "Venda",
                newName: "FormaPagamentoId");

            migrationBuilder.RenameColumn(
                name: "CodigoLive",
                table: "Venda",
                newName: "CondicaoPagamentoId");

            migrationBuilder.AddColumn<string>(
                name: "Observacoes",
                table: "Venda",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Venda",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "ValorBruto",
                table: "Venda",
                type: "decimal(65,30)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "VendaId",
                table: "Nfce",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Telefone",
                table: "EmpresaFiscal",
                type: "varchar(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(14)",
                oldMaxLength: 14,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "InscricaoEstadual",
                table: "EmpresaFiscal",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(15)",
                oldMaxLength: 15)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Complemento",
                table: "EmpresaFiscal",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(60)",
                oldMaxLength: 60,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "CNPJ",
                table: "EmpresaFiscal",
                type: "varchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(14)",
                oldMaxLength: 14)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "CEP",
                table: "EmpresaFiscal",
                type: "varchar(10)",
                maxLength: 10,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(8)",
                oldMaxLength: 8,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Venda_CondicaoPagamentoId",
                table: "Venda",
                column: "CondicaoPagamentoId");

            migrationBuilder.CreateIndex(
                name: "IX_Venda_FormaPagamentoId",
                table: "Venda",
                column: "FormaPagamentoId");

            migrationBuilder.CreateIndex(
                name: "IX_Venda_PedidoId",
                table: "Venda",
                column: "PedidoId");

            migrationBuilder.CreateIndex(
                name: "IX_Nfce_VendaId",
                table: "Nfce",
                column: "VendaId");

            migrationBuilder.AddForeignKey(
                name: "FK_Nfce_Venda_VendaId",
                table: "Nfce",
                column: "VendaId",
                principalTable: "Venda",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Venda_CondicaoPagamento_CondicaoPagamentoId",
                table: "Venda",
                column: "CondicaoPagamentoId",
                principalTable: "CondicaoPagamento",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Venda_FormaPagamento_FormaPagamentoId",
                table: "Venda",
                column: "FormaPagamentoId",
                principalTable: "FormaPagamento",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Venda_Pedido_PedidoId",
                table: "Venda",
                column: "PedidoId",
                principalTable: "Pedido",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Nfce_Venda_VendaId",
                table: "Nfce");

            migrationBuilder.DropForeignKey(
                name: "FK_Venda_CondicaoPagamento_CondicaoPagamentoId",
                table: "Venda");

            migrationBuilder.DropForeignKey(
                name: "FK_Venda_FormaPagamento_FormaPagamentoId",
                table: "Venda");

            migrationBuilder.DropForeignKey(
                name: "FK_Venda_Pedido_PedidoId",
                table: "Venda");

            migrationBuilder.DropIndex(
                name: "IX_Venda_CondicaoPagamentoId",
                table: "Venda");

            migrationBuilder.DropIndex(
                name: "IX_Venda_FormaPagamentoId",
                table: "Venda");

            migrationBuilder.DropIndex(
                name: "IX_Venda_PedidoId",
                table: "Venda");

            migrationBuilder.DropIndex(
                name: "IX_Nfce_VendaId",
                table: "Nfce");

            migrationBuilder.DropColumn(
                name: "Observacoes",
                table: "Venda");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Venda");

            migrationBuilder.DropColumn(
                name: "ValorBruto",
                table: "Venda");

            migrationBuilder.DropColumn(
                name: "VendaId",
                table: "Nfce");

            migrationBuilder.RenameColumn(
                name: "ValorTotal",
                table: "Venda",
                newName: "ValorVenda");

            migrationBuilder.RenameColumn(
                name: "PedidoId",
                table: "Venda",
                newName: "Quantidade");

            migrationBuilder.RenameColumn(
                name: "FormaPagamentoId",
                table: "Venda",
                newName: "OrdemVendaLive");

            migrationBuilder.RenameColumn(
                name: "CondicaoPagamentoId",
                table: "Venda",
                newName: "CodigoLive");

            migrationBuilder.AddColumn<int>(
                name: "ProdutoId",
                table: "Venda",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Telefone",
                table: "EmpresaFiscal",
                type: "varchar(14)",
                maxLength: 14,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(20)",
                oldMaxLength: 20,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "InscricaoEstadual",
                table: "EmpresaFiscal",
                type: "varchar(15)",
                maxLength: 15,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(20)",
                oldMaxLength: 20)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "Complemento",
                table: "EmpresaFiscal",
                type: "varchar(60)",
                maxLength: 60,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(100)",
                oldMaxLength: 100,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "CNPJ",
                table: "EmpresaFiscal",
                type: "varchar(14)",
                maxLength: 14,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(20)",
                oldMaxLength: 20)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "CEP",
                table: "EmpresaFiscal",
                type: "varchar(8)",
                maxLength: 8,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(10)",
                oldMaxLength: 10,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Venda_ProdutoId",
                table: "Venda",
                column: "ProdutoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Venda_Produto_ProdutoId",
                table: "Venda",
                column: "ProdutoId",
                principalTable: "Produto",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
