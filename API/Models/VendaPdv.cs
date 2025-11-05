using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ABrechozeiraApp.Models;

public class VendaPdv
{
    public int Id { get; set; }

    [StringLength(50)]
    public string? Codigo { get; set; }

    [ForeignKey("Pessoa")]
    public int? ClienteId { get; set; }
    public Pessoa? Cliente { get; set; }

    [StringLength(20)]
    public string Status { get; set; } = "Aberta"; // Aberta|Finalizada|Cancelada

    public DateTime DataVenda { get; set; } = DateTime.UtcNow;

    public decimal ValorBruto { get; set; }
    public decimal? Desconto { get; set; }
    public decimal ValorLiquido { get; set; }

    [StringLength(500)]
    public string? Observacao { get; set; }

    public int? UsuarioId { get; set; }
    public int? CaixaId { get; set; }

    public DateTime DataAlteracao { get; set; } = DateTime.UtcNow;
}

