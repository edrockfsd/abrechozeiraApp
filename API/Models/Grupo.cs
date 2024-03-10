﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ABrechozeiraApp.Models;

public partial class Grupo
{
    public int Id { get; set; }
    [StringLength(100)]
    public string Descricao { get; set; } = null!;   
}
