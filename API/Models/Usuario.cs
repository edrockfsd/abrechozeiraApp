using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ABrechozeiraApp.Models;

public partial class Usuario
{
    public int Id { get; set; }

    [StringLength(50)]
    public string? Login { get; set; }
    [StringLength(50)]
    public string? Senha { get; set; }
    [ForeignKey("NivelAcesso")]
    public int NivelAcessoID { get; set; }
    public virtual NivelAcesso? NivelAcesso { get; set; }
    [ForeignKey("Pessoa")]
    public int PessoaID { get; set; }
    public virtual Pessoa? Pessoa { get; set; } = null!;
}
