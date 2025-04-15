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
    [ForeignKey("ProdutoGrupo")]
    public int GrupoID { get; set; }

    public ProdutoGrupo? ProdutoGrupo { get; set; } = null!;

    public decimal? PrecoCusto { get; set; }
    [StringLength(50)]
    public string? Origem { get; set; }       

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

    [ForeignKey("PessoaGenero")]
    public int GeneroID { get; set; }

    public PessoaGenero? PessoaGenero { get; set; }
    
    [ForeignKey("ProdutoPerfil")]
    public int PerfilID { get; set; }

    public ProdutoPerfil? ProdutoPerfil { get; set; }

    // U -> Usado | N -> Novo
    public char? Condicao { get; set; } 

}
