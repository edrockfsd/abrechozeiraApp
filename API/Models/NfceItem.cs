using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ABrechozeiraApp.Models;

/// <summary>
/// Item da Nota Fiscal de Consumidor Eletrônica
/// </summary>
public class NfceItem
{
    public int Id { get; set; }

    [ForeignKey("Nfce")]
    public int NfceId { get; set; }
    public Nfce? Nfce { get; set; }

    /// <summary>
    /// Número sequencial do item na nota
    /// </summary>
    public int NumeroItem { get; set; }

    [ForeignKey("Produto")]
    public int? ProdutoId { get; set; }
    public Produto? Produto { get; set; }

    /// <summary>
    /// Código do produto (pode ser diferente do ID)
    /// </summary>
    [StringLength(60)]
    public string CodigoProduto { get; set; } = string.Empty;

    [StringLength(120)]
    public string Descricao { get; set; } = string.Empty;

    /// <summary>
    /// NCM - Nomenclatura Comum do Mercosul (8 dígitos)
    /// </summary>
    [StringLength(8)]
    public string NCM { get; set; } = "00000000";

    /// <summary>
    /// CFOP - Código Fiscal de Operações e Prestações
    /// </summary>
    [StringLength(4)]
    public string CFOP { get; set; } = "5102";

    /// <summary>
    /// Unidade de medida (UN, KG, M, etc)
    /// </summary>
    [StringLength(6)]
    public string Unidade { get; set; } = "UN";

    public decimal Quantidade { get; set; }

    public decimal ValorUnitario { get; set; }

    public decimal? ValorDesconto { get; set; }

    public decimal ValorTotal { get; set; }

    // Tributação
    /// <summary>
    /// Código de Situação da Operação no Simples Nacional
    /// </summary>
    [StringLength(3)]
    public string CSOSN { get; set; } = "102";

    /// <summary>
    /// Origem da mercadoria: 0=Nacional, 1=Estrangeira importação direta, etc
    /// </summary>
    public int OrigemMercadoria { get; set; } = 0;
}
