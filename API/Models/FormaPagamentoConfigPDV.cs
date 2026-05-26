using System.ComponentModel.DataAnnotations.Schema;

namespace ABrechozeiraApp.Models;

public class FormaPagamentoConfigPDV
{
    public int Id { get; set; }
    [ForeignKey("FormaPagamento")] public int FormaPagamentoId { get; set; }
    public FormaPagamento? FormaPagamento { get; set; }
    public bool ExibirNoPDV { get; set; } = true;
    public bool PermiteParcelamento { get; set; } = false;
    public int? MaxParcelas { get; set; }
    public decimal? TaxaAdmPerc { get; set; }
}

