using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ABrechozeiraApp.Models;

public partial class Endereco
{
    public int Id { get; set; }
    
    [ForeignKey("Pessoa")]
    public int PessoaID { get; set; }
    public Pessoa? Pessoa { get; set; }

    [ForeignKey("TipoEndereco")]
    public int TipoEnderecoId { get; set; }
    public TipoEndereco? TipoEndereco { get; set; }

    [StringLength(8)]
    public string? CEP { get; set; }

    [StringLength(100)]
    public string Logradouro { get; set; } = string.Empty;
    [StringLength(8)]
    public string Unidade { get; set; } = string.Empty;
    [StringLength(50)]
    public string? Complemento { get; set; }
    [StringLength(50)]
    public string Bairro { get; set; } = string.Empty;
    [StringLength(50)]
    public string Localidade { get; set; } = string.Empty;
    public int CodigoLocalidadeIBGE { get; set; }
    [StringLength(30)]
    public string Estado { get; set; } = string.Empty;
    public string? Observacoes {  get; set; } 
    public DateTime? DataAlteracao { get; set; }

    

}
