using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ABrechozeiraApp.Models;

public class VendaPdvPagamento
{
    public int Id { get; set; }

    [ForeignKey("VendaPdv")] public int VendaPdvId { get; set; }
    public VendaPdv? VendaPdv { get; set; }

    [ForeignKey("FormaPagamento")] public int FormaPagamentoId { get; set; }
    public FormaPagamento? FormaPagamento { get; set; }

    [ForeignKey("CondicaoPagamento")] public int? CondicaoPagamentoId { get; set; }
    public CondicaoPagamento? CondicaoPagamento { get; set; }

    public decimal Valor { get; set; }
    public string? Observacao { get; set; }
    public string? TransacaoId { get; set; }
    public DateTime DataRegistro { get; set; } = DateTime.UtcNow;
}

