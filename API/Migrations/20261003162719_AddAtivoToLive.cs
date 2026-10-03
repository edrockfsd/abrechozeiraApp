using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABrechozeiraApp.Migrations
{
    /// <inheritdoc />
    public partial class AddAtivoToLive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                SET @colCount = (SELECT COUNT(*) FROM information_schema.COLUMNS 
                                 WHERE TABLE_SCHEMA = DATABASE() AND TABLE_NAME = 'Live' AND COLUMN_NAME = 'Ativo');
                SET @sql = IF(@colCount = 0, 'ALTER TABLE `Live` ADD COLUMN `Ativo` TINYINT(1) NOT NULL DEFAULT 1', 'SELECT 1');
                PREPARE stmt FROM @sql;
                EXECUTE stmt;
                DEALLOCATE PREPARE stmt;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Ativo",
                table: "Live");
        }
    }
}
