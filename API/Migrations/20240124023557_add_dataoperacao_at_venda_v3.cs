using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABrechozeiraApp.Migrations
{
    /// <inheritdoc />
    public partial class add_dataoperacao_at_venda_v3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            

            

           
            

           

            migrationBuilder.AddColumn<string>(
                name: "Origem",
                table: "Venda",
                type: "varchar(1)",
                maxLength: 1,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

migrationBuilder.DropTable(
                name: "Origem");

 migrationBuilder.DropIndex(
                name: "IX_Venda_OrigemID",
                table: "Venda");
migrationBuilder.DropColumn(
                name: "DataOperacao",
                table: "Venda");
            migrationBuilder.DropColumn(
                           name: "OrigemID",
                           table: "Venda");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            
        }
    }
}
