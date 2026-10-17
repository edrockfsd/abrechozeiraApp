using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABrechozeiraApp.Migrations
{
    /// <inheritdoc />
    public partial class AddOrigemPagamentoEnvioLoteMap : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OrigemPagamento",
                table: "tb_enviolote_map",
                type: "varchar(20)",
                maxLength: 20,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "PagoEm",
                table: "tb_enviolote_map",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PagoManualPor",
                table: "tb_enviolote_map",
                type: "varchar(255)",
                maxLength: 255,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrigemPagamento",
                table: "tb_enviolote_map");

            migrationBuilder.DropColumn(
                name: "PagoEm",
                table: "tb_enviolote_map");

            migrationBuilder.DropColumn(
                name: "PagoManualPor",
                table: "tb_enviolote_map");
        }
    }
}
