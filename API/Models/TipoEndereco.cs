using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ABrechozeiraApp.Models;

public partial class TipoEndereco
{
    public int Id { get; set; }
    [StringLength(50)]
    public string Descricao { get; set; } = null!;

}
