using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ABrechozeiraApp.Models;

public partial class CondicaoPagamento
{
    public int Id { get; set; }

    public string Descricao { get; set; } = string.Empty;
    public DateTime? DataAlteracao { get; set; }

    [ForeignKey("Usuario")]
    public int UsuarioModificacaoId { get; set; }
    public Usuario? UsuarioModificacao { get; set; }

}
