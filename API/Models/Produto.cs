using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ABrechozeiraApp.Models;

public partial class Produto
{
    public int Id { get; set; }
    [StringLength(100)]
    public string Descricao { get; set; } = null!;
    [StringLength(50)]
    public string? Tamanho { get; set; }    
    public int GrupoID { get; set; }

    public ProdutoGrupo? ProdutoGrupo { get; set; } = null!;

    public decimal? PrecoCusto { get; set; }
    [StringLength(50)]
    public string? Origem { get; set; }       

    public int? PessoaPertenceID { get; set; } = null;

    public Pessoa? PessoaPertence { get; set; }

    public DateTime? DataCompra { get; set; }

    public DateTime? DataAlteracao { get; set; }

    [ForeignKey("Usuario")]
    public int UsuarioModificacaoId { get; set; }
    public Usuario? UsuarioModificacao { get; set; }
    [ForeignKey("ProdutoStatus")]
    public int StatusId { get; set; }

    public ProdutoStatus? ProdutoStatus { get; set; }

    [ForeignKey("Marca")]
    public int? MarcaId { get; set; }

    public ProdutoMarca? Marca { get; set; }

    public decimal? PrecoVenda { get; set; }

    public char? Genero { get; set; }

    public char? Perfil { get; set; }

}
