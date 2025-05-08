using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ABrechozeiraApp.Models;

public partial class PedidoProduto
{
    public int Id { get; set; }

    [ForeignKey("Pedido")]
    public int PedidoId { get; set; }

    public Pedido? Pedido { get; set; }

    [ForeignKey("Produto")]
    public int ProdutoId { get; set; }

    public Produto? Produto { get; set; }

    public int Quantidade { get; set; }

    public decimal? DescontoValor { get; set; } = 0;

    public decimal? ValorFinalProduto { get; set; } = 0;

    public DateTime? DataAlteracao { get; set; }

    [ForeignKey("Usuario")]
    public int UsuarioModificacaoId { get; set; }
    public Usuario? UsuarioModificacao { get; set; }

}
