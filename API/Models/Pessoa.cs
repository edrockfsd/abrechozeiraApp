﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ABrechozeiraApp.Models;

public partial class Pessoa
{
    public int Id { get; set; }

    [StringLength(50)]
    public string? Nome { get; set; }

    public DateTime? DataNascimento { get; set; }
    [StringLength(100)]
    public string? Email { get; set; }
    [StringLength(13)]
    public string? Telefone { get; set; }
    [ForeignKey("PessoaGenero")]
    public int PessoaGeneroId { get; set; }
    public PessoaGenero? PessoaGenero { get; set; } = null!;
    [StringLength(50)]
    public string? CPF { get; set; }
    public string? RG { get; set; }

    public string? NickName { get; set; }

    public string? Observacoes { get; set; }
    public DateTime? DataInclusao { get; set; } = DateTime.Now;
    [ForeignKey("PessoaCategoria")]  
    public int PessoaCategoriaId { get; set; }
    public PessoaCategoria? PessoaCategoria { get; set; } = null!;
    [ForeignKey("PessoaTipo")]
    public int PessoaTipoId { get; set; }
    public PessoaTipo? PessoaTipo { get; set; } = null!;

    [ForeignKey("PessoaStatus")]
    public int StatusId { get; set; }

    public PessoaStatus? PessoaStatus { get; set; }

}
