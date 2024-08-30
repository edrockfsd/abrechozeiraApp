using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ABrechozeiraApp.Models;

public partial class Arremate
{
    public int Id { get; set; }

    [ForeignKey("Live")]
    public int LiveId { get; set; }

    public Live? Live { get; set; }


    [ForeignKey("Produto")]
    public int ProdutoId { get; set; }

    public Produto? Produto { get; set; }

    public int? CodigoLive { get; set; }

    [StringLength(50)]
    public string Arrematante { get; set; } = string.Empty;

    public decimal? ValorArremate { get; set; }

    public string? Observacoes { get; set; }

    public DateTime DataArremate { get; set; }

    public DateTime DataAlteracao { get; set; }

    [ForeignKey("Usuario")]
    public int UsuarioModificacaoId { get; set; }
    public Usuario? UsuarioModificacao { get; set; }

}

