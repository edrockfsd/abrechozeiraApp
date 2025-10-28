using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ABrechozeiraApp.Models;

public partial class Venda
{
    public int Id { get; set; }

    public int Quantidade { get; set; }

    public decimal ValorVenda { get; set; }

    public decimal? Desconto { get; set; }
    [ForeignKey("Pessoa")]
    public int ClienteId { get; set; }

    public Pessoa? Cliente { get; set; }

    [ForeignKey("Produto")]
    public int ProdutoId { get; set; }

    public Produto? Produto { get; set; }
    
    public int? OrigemID { get; set; }

    public Origem? Origem { get; set; }
    //Faz referência ao código do produto atribuído no momento da live
    public int? CodigoLive { get; set; }
    
    public int? OrdemVendaLive { get; set; }
    
    [ForeignKey("Live")]
    public int? LiveId { get; set; }

    public Live? Live { get; set; }

    public DateTime? DataVenda { get; set; }

    public DateTime? DataPagamento { get; set; }    

    public DateTime? DataAlteracao { get; set; }

    
}
