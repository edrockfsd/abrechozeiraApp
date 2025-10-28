using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ABrechozeiraApp.Models;

[Index(nameof(PedidoCodigo), IsUnique = true)]
public partial class Pedido
{
    public int Id { get; set; }

    public int PedidoCodigo { get; set; }

    public DateTime DataLancamento { get; set; }

    [ForeignKey("Pessoa")]
    public int ClienteID { get; set; }
    public Pessoa? Cliente { get; set; }

    public decimal? DescontoPorcentagem { get; set; } = 0;

    public decimal? ValorFrete { get; set; } = 0;

    [ForeignKey("PedidoStatus")]
    public int PedidoStatusID { get; set; }
    public PedidoStatus? PedidoStatus { get; set; }

    public decimal? ValorTotal { get; set; } = 0;

    [ForeignKey("CondicaoPagamento")]
    public int? CondicaoPagamentoID { get; set; }
    public CondicaoPagamento? CondicaoPagamento { get; set; }


    [ForeignKey("FormaPagamento")]
    public int? FormaPagamentoID { get; set; }
    public FormaPagamento? FormaPagamento { get; set; }

    [ForeignKey("Endereco")]
    public int? EnderecoEntregaID { get; set; }
    public Endereco? EnderecoEntrega { get; set; }

    public string? Observacoes { get; set; }
    public DateTime DataAlteracao { get; set; }

    

}
