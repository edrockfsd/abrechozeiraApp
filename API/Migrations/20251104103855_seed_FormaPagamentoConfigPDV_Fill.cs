using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ABrechozeiraApp.Migrations
{
    /// <inheritdoc />
    public partial class seed_FormaPagamentoConfigPDV_Fill : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                INSERT INTO FormaPagamentoConfigPDV (FormaPagamentoId, ExibirNoPDV, PermiteParcelamento)
                SELECT fp.Id, 1, 0
                FROM FormaPagamento fp
                LEFT JOIN FormaPagamentoConfigPDV cfg ON cfg.FormaPagamentoId = fp.Id
                WHERE cfg.Id IS NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                DELETE cfg FROM FormaPagamentoConfigPDV cfg
                WHERE NOT EXISTS (
                    SELECT 1 FROM VendaPdvPagamento vpp WHERE vpp.FormaPagamentoId = cfg.FormaPagamentoId
                );
            ");
        }
    }
}
