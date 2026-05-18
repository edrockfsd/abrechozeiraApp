using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ABrechozeiraApp.Models;

/// <summary>
/// Nota Fiscal de Consumidor Eletrônica
/// </summary>
public class Nfce
{
    public int Id { get; set; }

    /// <summary>
    /// Número da nota na série
    /// </summary>
    public int Numero { get; set; }

    public int Serie { get; set; } = 1;

    /// <summary>
    /// Chave de acesso da NFC-e (44 dígitos)
    /// </summary>
    [StringLength(44)]
    public string? ChaveAcesso { get; set; }

    /// <summary>
    /// Protocolo de autorização da SEFAZ
    /// </summary>
    [StringLength(20)]
    public string? Protocolo { get; set; }

    /// <summary>
    /// Status: Pendente, Autorizada, Cancelada, Denegada, Rejeitada
    /// </summary>
    [StringLength(20)]
    public string Status { get; set; } = "Pendente";

    /// <summary>
    /// Código de retorno da SEFAZ
    /// </summary>
    public int? CodigoRetorno { get; set; }

    /// <summary>
    /// Mensagem de retorno da SEFAZ
    /// </summary>
    [StringLength(500)]
    public string? MensagemRetorno { get; set; }

    // Valores
    public decimal ValorProdutos { get; set; }
    public decimal? ValorDesconto { get; set; }
    public decimal ValorTotal { get; set; }

    // Relacionamento com VendaPdv (opcional)
    [ForeignKey("VendaPdv")]
    public int? VendaPdvId { get; set; }
    public VendaPdv? VendaPdv { get; set; }

    // Relacionamento com Pedido (opcional)
    [ForeignKey("Pedido")]
    public int? PedidoId { get; set; }
    public Pedido? Pedido { get; set; }

    // Relacionamento com Cliente (opcional)
    [ForeignKey("Cliente")]
    public int? ClienteId { get; set; }
    public Pessoa? Cliente { get; set; }

    // Dados do destinatário (para quando não há cliente cadastrado)
    [StringLength(14)]
    public string? ClienteCpfCnpj { get; set; }

    [StringLength(100)]
    public string? ClienteNome { get; set; }

    // XMLs
    [Column(TypeName = "LONGTEXT")]
    public string? XmlEnvio { get; set; }

    [Column(TypeName = "LONGTEXT")]
    public string? XmlRetorno { get; set; }

    // Cancelamento
    public DateTime? DataCancelamento { get; set; }

    [StringLength(255)]
    public string? JustificativaCancelamento { get; set; }

    [StringLength(20)]
    public string? ProtocoloCancelamento { get; set; }

    // Datas
    public DateTime DataEmissao { get; set; } = DateTime.UtcNow;
    public DateTime? DataAutorizacao { get; set; }

    // Usuário que emitiu
    public int? UsuarioId { get; set; }

    // Ambiente: 1=Produção, 2=Homologação
    public int Ambiente { get; set; } = 2;

    // Itens da nota
    public virtual ICollection<NfceItem>? Itens { get; set; }

    // Pagamentos da nota
    public virtual ICollection<NfcePagamento>? Pagamentos { get; set; }
}
