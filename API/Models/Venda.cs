using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ABrechozeiraApp.Models;

public partial class Venda
{
    public int Id { get; set; }

    // Vínculo com Pedido de origem
    [ForeignKey("Pedido")]
    public int PedidoId { get; set; }
    public Pedido? Pedido { get; set; }

    // Dados financeiros da venda
    public decimal ValorBruto { get; set; }
    public decimal? Desconto { get; set; }
    public decimal ValorTotal { get; set; }

    // Status da venda: Pendente | Confirmada | Faturada | Cancelada
    [StringLength(20)]
    public string Status { get; set; } = "Confirmada";

    [ForeignKey("Pessoa")]
    public int ClienteId { get; set; }
    public Pessoa? Cliente { get; set; }

    public int? OrigemID { get; set; }
    public Origem? Origem { get; set; }

    [ForeignKey("Live")]
    public int? LiveId { get; set; }
    public Live? Live { get; set; }

    [ForeignKey("FormaPagamento")]
    public int? FormaPagamentoId { get; set; }
    public FormaPagamento? FormaPagamento { get; set; }

    [ForeignKey("CondicaoPagamento")]
    public int? CondicaoPagamentoId { get; set; }
    public CondicaoPagamento? CondicaoPagamento { get; set; }

    public DateTime? DataVenda { get; set; }
    public DateTime? DataPagamento { get; set; }
    public DateTime? DataAlteracao { get; set; }

    [StringLength(500)]
    public string? Observacoes { get; set; }
}
