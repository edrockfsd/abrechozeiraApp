using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ABrechozeiraApp.Models;

public class CaixaMovimento
{
    public int Id { get; set; }
    [ForeignKey("Caixa")] public int CaixaId { get; set; }
    public Caixa? Caixa { get; set; }
    public string Tipo { get; set; } = "Venda"; // Suprimento|Sangria|Venda|Estorno
    public decimal Valor { get; set; }
    public int? ReferenciaId { get; set; }
    public string? Observacao { get; set; }
    public DateTime DataRegistro { get; set; } = DateTime.UtcNow;
}

