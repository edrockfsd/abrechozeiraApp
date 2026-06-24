using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABrechozeiraApp.Migrations
{
    /// <inheritdoc />
    public partial class AddWhatsAppAndSheetFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EmailCotacaoEnviado",
                table: "tb_enviolote_map",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "EmailRastreioEnviado",
                table: "tb_enviolote_map",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "LinkCheckout",
                table: "tb_enviolote_map",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "PrecoPAC",
                table: "tb_enviolote_map",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PrecoRecomendado",
                table: "tb_enviolote_map",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PrecoSEDEX",
                table: "tb_enviolote_map",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ServicoRecomendado",
                table: "tb_enviolote_map",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "WhatsAppCotacaoEnviado",
                table: "tb_enviolote_map",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "WhatsAppRastreioEnviado",
                table: "tb_enviolote_map",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "GoogleSheetUrl",
                table: "Live",
                type: "varchar(500)",
                maxLength: 500,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<bool>(
                name: "ImportadoPlanilha",
                table: "Arremate",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailCotacaoEnviado",
                table: "tb_enviolote_map");

            migrationBuilder.DropColumn(
                name: "EmailRastreioEnviado",
                table: "tb_enviolote_map");

            migrationBuilder.DropColumn(
                name: "LinkCheckout",
                table: "tb_enviolote_map");

            migrationBuilder.DropColumn(
                name: "PrecoPAC",
                table: "tb_enviolote_map");

            migrationBuilder.DropColumn(
                name: "PrecoRecomendado",
                table: "tb_enviolote_map");

            migrationBuilder.DropColumn(
                name: "PrecoSEDEX",
                table: "tb_enviolote_map");

            migrationBuilder.DropColumn(
                name: "ServicoRecomendado",
                table: "tb_enviolote_map");

            migrationBuilder.DropColumn(
                name: "WhatsAppCotacaoEnviado",
                table: "tb_enviolote_map");

            migrationBuilder.DropColumn(
                name: "WhatsAppRastreioEnviado",
                table: "tb_enviolote_map");

            migrationBuilder.DropColumn(
                name: "GoogleSheetUrl",
                table: "Live");

            migrationBuilder.DropColumn(
                name: "ImportadoPlanilha",
                table: "Arremate");
        }
    }
}
