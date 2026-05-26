using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABrechozeiraApp.Migrations
{
    /// <inheritdoc />
    public partial class alt_usuarios_user : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Arremate_Usuario_UsuarioModificacaoId",
                table: "Arremate");

            migrationBuilder.DropForeignKey(
                name: "FK_CondicaoPagamento_Usuario_UsuarioModificacaoId",
                table: "CondicaoPagamento");

            migrationBuilder.DropForeignKey(
                name: "FK_Endereco_Usuario_UsuarioModificacaoId",
                table: "Endereco");

            migrationBuilder.DropForeignKey(
                name: "FK_Estoque_Usuario_UsuarioModificacaoId",
                table: "Estoque");

            migrationBuilder.DropForeignKey(
                name: "FK_FormaPagamento_Usuario_UsuarioModificacaoId",
                table: "FormaPagamento");

            migrationBuilder.DropForeignKey(
                name: "FK_Live_Usuario_UsuarioModificacaoId",
                table: "Live");

            migrationBuilder.DropForeignKey(
                name: "FK_Pedido_Usuario_UsuarioModificacaoId",
                table: "Pedido");

            migrationBuilder.DropForeignKey(
                name: "FK_PedidoProduto_Usuario_UsuarioModificacaoId",
                table: "PedidoProduto");

            migrationBuilder.DropForeignKey(
                name: "FK_PedidoStatus_Usuario_UsuarioModificacaoId",
                table: "PedidoStatus");

            migrationBuilder.DropForeignKey(
                name: "FK_Produto_Usuario_UsuarioModificacaoId",
                table: "Produto");

            migrationBuilder.DropForeignKey(
                name: "FK_Venda_Usuario_UsuarioModificacaoId",
                table: "Venda");

            migrationBuilder.DropIndex(
                name: "IX_Venda_UsuarioModificacaoId",
                table: "Venda");

            migrationBuilder.DropIndex(
                name: "IX_Produto_UsuarioModificacaoId",
                table: "Produto");

            migrationBuilder.DropIndex(
                name: "IX_PedidoStatus_UsuarioModificacaoId",
                table: "PedidoStatus");

            migrationBuilder.DropIndex(
                name: "IX_PedidoProduto_UsuarioModificacaoId",
                table: "PedidoProduto");

            migrationBuilder.DropIndex(
                name: "IX_Pedido_UsuarioModificacaoId",
                table: "Pedido");

            migrationBuilder.DropIndex(
                name: "IX_Live_UsuarioModificacaoId",
                table: "Live");

            migrationBuilder.DropIndex(
                name: "IX_FormaPagamento_UsuarioModificacaoId",
                table: "FormaPagamento");

            migrationBuilder.DropIndex(
                name: "IX_Estoque_UsuarioModificacaoId",
                table: "Estoque");

            migrationBuilder.DropIndex(
                name: "IX_Endereco_UsuarioModificacaoId",
                table: "Endereco");

            migrationBuilder.DropIndex(
                name: "IX_CondicaoPagamento_UsuarioModificacaoId",
                table: "CondicaoPagamento");

            migrationBuilder.DropIndex(
                name: "IX_Arremate_UsuarioModificacaoId",
                table: "Arremate");

            migrationBuilder.DropColumn(
                name: "UsuarioModificacaoId",
                table: "Venda");

            migrationBuilder.DropColumn(
                name: "UsuarioModificacaoId",
                table: "Produto");

            migrationBuilder.DropColumn(
                name: "UsuarioModificacaoId",
                table: "PedidoStatus");

            migrationBuilder.DropColumn(
                name: "UsuarioModificacaoId",
                table: "PedidoProduto");

            migrationBuilder.DropColumn(
                name: "UsuarioModificacaoId",
                table: "Pedido");

            migrationBuilder.DropColumn(
                name: "UsuarioModificacaoId",
                table: "Live");

            migrationBuilder.DropColumn(
                name: "UsuarioModificacaoId",
                table: "FormaPagamento");

            migrationBuilder.DropColumn(
                name: "UsuarioModificacaoId",
                table: "Estoque");

            migrationBuilder.DropColumn(
                name: "UsuarioModificacaoId",
                table: "Endereco");

            migrationBuilder.DropColumn(
                name: "UsuarioModificacaoId",
                table: "CondicaoPagamento");

            migrationBuilder.DropColumn(
                name: "UsuarioModificacaoId",
                table: "Arremate");

            migrationBuilder.RenameColumn(
                name: "LiveSessionID",
                table: "ComentarioLive",
                newName: "LiveSessionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "LiveSessionId",
                table: "ComentarioLive",
                newName: "LiveSessionID");

            migrationBuilder.AddColumn<int>(
                name: "UsuarioModificacaoId",
                table: "Venda",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UsuarioModificacaoId",
                table: "Produto",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UsuarioModificacaoId",
                table: "PedidoStatus",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UsuarioModificacaoId",
                table: "PedidoProduto",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UsuarioModificacaoId",
                table: "Pedido",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UsuarioModificacaoId",
                table: "Live",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UsuarioModificacaoId",
                table: "FormaPagamento",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UsuarioModificacaoId",
                table: "Estoque",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UsuarioModificacaoId",
                table: "Endereco",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UsuarioModificacaoId",
                table: "CondicaoPagamento",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UsuarioModificacaoId",
                table: "Arremate",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Venda_UsuarioModificacaoId",
                table: "Venda",
                column: "UsuarioModificacaoId");

            migrationBuilder.CreateIndex(
                name: "IX_Produto_UsuarioModificacaoId",
                table: "Produto",
                column: "UsuarioModificacaoId");

            migrationBuilder.CreateIndex(
                name: "IX_PedidoStatus_UsuarioModificacaoId",
                table: "PedidoStatus",
                column: "UsuarioModificacaoId");

            migrationBuilder.CreateIndex(
                name: "IX_PedidoProduto_UsuarioModificacaoId",
                table: "PedidoProduto",
                column: "UsuarioModificacaoId");

            migrationBuilder.CreateIndex(
                name: "IX_Pedido_UsuarioModificacaoId",
                table: "Pedido",
                column: "UsuarioModificacaoId");

            migrationBuilder.CreateIndex(
                name: "IX_Live_UsuarioModificacaoId",
                table: "Live",
                column: "UsuarioModificacaoId");

            migrationBuilder.CreateIndex(
                name: "IX_FormaPagamento_UsuarioModificacaoId",
                table: "FormaPagamento",
                column: "UsuarioModificacaoId");

            migrationBuilder.CreateIndex(
                name: "IX_Estoque_UsuarioModificacaoId",
                table: "Estoque",
                column: "UsuarioModificacaoId");

            migrationBuilder.CreateIndex(
                name: "IX_Endereco_UsuarioModificacaoId",
                table: "Endereco",
                column: "UsuarioModificacaoId");

            migrationBuilder.CreateIndex(
                name: "IX_CondicaoPagamento_UsuarioModificacaoId",
                table: "CondicaoPagamento",
                column: "UsuarioModificacaoId");

            migrationBuilder.CreateIndex(
                name: "IX_Arremate_UsuarioModificacaoId",
                table: "Arremate",
                column: "UsuarioModificacaoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Arremate_Usuario_UsuarioModificacaoId",
                table: "Arremate",
                column: "UsuarioModificacaoId",
                principalTable: "Usuario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CondicaoPagamento_Usuario_UsuarioModificacaoId",
                table: "CondicaoPagamento",
                column: "UsuarioModificacaoId",
                principalTable: "Usuario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Endereco_Usuario_UsuarioModificacaoId",
                table: "Endereco",
                column: "UsuarioModificacaoId",
                principalTable: "Usuario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Estoque_Usuario_UsuarioModificacaoId",
                table: "Estoque",
                column: "UsuarioModificacaoId",
                principalTable: "Usuario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_FormaPagamento_Usuario_UsuarioModificacaoId",
                table: "FormaPagamento",
                column: "UsuarioModificacaoId",
                principalTable: "Usuario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Live_Usuario_UsuarioModificacaoId",
                table: "Live",
                column: "UsuarioModificacaoId",
                principalTable: "Usuario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Pedido_Usuario_UsuarioModificacaoId",
                table: "Pedido",
                column: "UsuarioModificacaoId",
                principalTable: "Usuario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PedidoProduto_Usuario_UsuarioModificacaoId",
                table: "PedidoProduto",
                column: "UsuarioModificacaoId",
                principalTable: "Usuario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_PedidoStatus_Usuario_UsuarioModificacaoId",
                table: "PedidoStatus",
                column: "UsuarioModificacaoId",
                principalTable: "Usuario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Produto_Usuario_UsuarioModificacaoId",
                table: "Produto",
                column: "UsuarioModificacaoId",
                principalTable: "Usuario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Venda_Usuario_UsuarioModificacaoId",
                table: "Venda",
                column: "UsuarioModificacaoId",
                principalTable: "Usuario",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
