using System;
using System.ComponentModel.DataAnnotations;

namespace ABrechozeiraApp.Models;

public class Caixa
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public DateTime DataAbertura { get; set; } = DateTime.UtcNow;
    public decimal SaldoInicial { get; set; }
    public DateTime? DataFechamento { get; set; }
    public decimal? SaldoFechamento { get; set; }
    [StringLength(300)] public string? Observacao { get; set; }
}

