using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ABrechozeiraApp.Models;


/// <summary>
/// Representa um único comentário de uma live do Instagram.
/// </summary>
[Table("ComentarioLive")]
public partial class ComentarioLive
{
    [Key]
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Username { get; set; } = string.Empty;

    [Required]    
    public string CommentText { get; set; } = string.Empty;

    [Required]
    public DateTime CommentTimestamp { get; set; }

    [Required]
    public DateTime CreatedAt { get; set; }

    public long? LiveSessionId { get; set; }
}
