using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ABrechozeiraApp.Models;

public class VendaPdvItem
{
    public int Id { get; set; }

    [ForeignKey("VendaPdv")] public int VendaPdvId { get; set; }
    public VendaPdv? VendaPdv { get; set; }

    [ForeignKey("Produto")] public int? ProdutoId { get; set; }
    public Produto? Produto { get; set; }

    [StringLength(200)] public string DescricaoItem { get; set; } = string.Empty;

    public decimal Quantidade { get; set; }
    public decimal PrecoUnitario { get; set; }
    public decimal? DescontoValor { get; set; }
    public decimal? DescontoPerc { get; set; }
    public decimal Total { get; set; }

    [StringLength(50)] public string? CodigoEstoque { get; set; }
}

