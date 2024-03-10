using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ABrechozeiraApp.Models;

public partial class NivelAcesso
{
    public int Id { get; set; }
    [StringLength(200)]
    public string Descricao { get; set; } = null!;
}
