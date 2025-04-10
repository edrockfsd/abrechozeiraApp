using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABrechozeiraApp.Migrations
{
    /// <inheritdoc />
    public partial class tb_pessoa_add_cpf_e_rg : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Endereco_TipoEndereco_TipoEnderecoID",
                table: "Endereco");

            migrationBuilder.RenameColumn(
                name: "TipoEnderecoID",
                table: "Endereco",
                newName: "TipoEnderecoId");

            migrationBuilder.RenameIndex(
                name: "IX_Endereco_TipoEnderecoID",
                table: "Endereco",
                newName: "IX_Endereco_TipoEnderecoId");

            migrationBuilder.AlterColumn<string>(
                name: "NickName",
                table: "Pessoa",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(50)",
                oldMaxLength: 50,
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "CPF",
                table: "Pessoa",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "RG",
                table: "Pessoa",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddForeignKey(
                name: "FK_Endereco_TipoEndereco_TipoEnderecoId",
                table: "Endereco",
                column: "TipoEnderecoId",
                principalTable: "TipoEndereco",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Endereco_TipoEndereco_TipoEnderecoId",
                table: "Endereco");

            migrationBuilder.DropColumn(
                name: "CPF",
                table: "Pessoa");

            migrationBuilder.DropColumn(
                name: "RG",
                table: "Pessoa");

            migrationBuilder.RenameColumn(
                name: "TipoEnderecoId",
                table: "Endereco",
                newName: "TipoEnderecoID");

            migrationBuilder.RenameIndex(
                name: "IX_Endereco_TipoEnderecoId",
                table: "Endereco",
                newName: "IX_Endereco_TipoEnderecoID");

            migrationBuilder.AlterColumn<string>(
                name: "NickName",
                table: "Pessoa",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddForeignKey(
                name: "FK_Endereco_TipoEndereco_TipoEnderecoID",
                table: "Endereco",
                column: "TipoEnderecoID",
                principalTable: "TipoEndereco",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
