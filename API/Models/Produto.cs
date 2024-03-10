using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ABrechozeiraApp.Models;

public partial class Produto
{
    public int Id { get; set; }
    [StringLength(100)]
    public string Descricao { get; set; } = null!;
    [StringLength(50)]
    public string? Tamanho { get; set; }
    [StringLength(50)]
    public int GrupoID { get; set; }

    public Grupo Grupo { get; set; } = null!;

    public decimal? PrecoCusto { get; set; }

    public char? Origem { get; set; }

    public int? CodigoEstoque { get; set; }

    public int? CodigoLive { get; set; }



}
