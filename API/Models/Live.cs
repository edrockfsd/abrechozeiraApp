using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ABrechozeiraApp.Models;

public partial class Live
{
    public int Id { get; set; }
    [StringLength(50)]
    public string Titulo { get; set; } = null!;
    
    public string? Observacoes { get; set; }

    public DateTime DataLive { get; set; }

    public DateTime DataAlteracao { get; set; }

    [ForeignKey("Usuario")]
    public int UsuarioModificacaoId { get; set; }
    public Usuario? UsuarioModificacao { get; set; }
}

