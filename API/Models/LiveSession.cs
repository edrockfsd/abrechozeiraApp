using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ABrechozeiraApp.Models;

/// <summary>
/// Representa uma transmissão ao vivo (live) do Instagram.
/// </summary>
public partial class LiveSession
{
    [Key] // Marca a propriedade Id como a chave primária
    public int Id { get; set; }

    /// <summary>
    /// O ID da transmissão ao vivo, fornecido pela API do Instagram.
    /// É um número grande, então usamos 'long'.
    /// </summary>
    [Required]
    public long LiveVideoId { get; set; }

    /// <summary>
    /// O status atual da live. Ex: "active" ou "ended".
    /// </summary>
    [Required]
    [StringLength(50)]
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Data e hora em que a live foi iniciada.
    /// </summary>
    [Required]
    public DateTime StartedAt { get; set; }

    /// <summary>
    /// Data e hora em que a live foi finalizada (pode ser nulo se ainda estiver ativa).
    /// </summary>
    public DateTime? EndedAt { get; set; } // O '?' permite que o valor seja nulo
    
}
