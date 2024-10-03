using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ABrechozeiraApp.Models;

public partial class PessoaGenero
{
    public int Id { get; set; }
    [StringLength(1)]
    public string Sigla { get; set; } = null!;
    [StringLength(50)]
    public string Descricao { get; set; } = null!;

}
