using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ABrechozeiraApp.Models;

/// <summary>
/// Configurações fiscais para emissão de NFC-e
/// </summary>
public class EmpresaFiscal
{
    public int Id { get; set; }

    // Dados da Empresa
    [Required]
    [StringLength(14)]
    public string CNPJ { get; set; } = string.Empty;

    [Required]
    [StringLength(15)]
    public string InscricaoEstadual { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string RazaoSocial { get; set; } = string.Empty;

    [StringLength(100)]
    public string? NomeFantasia { get; set; }

    // Endereço
    [StringLength(100)]
    public string? Logradouro { get; set; }

    [StringLength(10)]
    public string? Numero { get; set; }

    [StringLength(60)]
    public string? Complemento { get; set; }

    [StringLength(60)]
    public string? Bairro { get; set; }

    [StringLength(7)]
    public string? CodigoMunicipio { get; set; }

    [StringLength(60)]
    public string? Municipio { get; set; }

    [StringLength(2)]
    public string UF { get; set; } = "RS";

    [StringLength(8)]
    public string? CEP { get; set; }

    [StringLength(14)]
    public string? Telefone { get; set; }

    // Configurações NFC-e
    /// <summary>
    /// 1=Produção, 2=Homologação
    /// </summary>
    public int Ambiente { get; set; } = 2;

    /// <summary>
    /// CRT: 1=Simples Nacional, 4=MEI
    /// </summary>
    public int CRT { get; set; } = 4;

    public int Serie { get; set; } = 1;

    public int ProximoNumero { get; set; } = 1;

    /// <summary>
    /// 1=Normal, 9=Contingência Offline
    /// </summary>
    public int TipoEmissao { get; set; } = 1;

    // CSC (Código de Segurança do Contribuinte)
    [StringLength(36)]
    public string? CSC { get; set; }

    [StringLength(6)]
    public string? CSCId { get; set; }

    // Certificado Digital
    [StringLength(500)]
    public string? CertificadoPath { get; set; }

    [StringLength(100)]
    public string? CertificadoSenha { get; set; }

    public DateTime? CertificadoValidade { get; set; }

    public bool Ativo { get; set; } = true;

    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    public DateTime DataAlteracao { get; set; } = DateTime.UtcNow;
}
