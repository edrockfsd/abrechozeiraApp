using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Text.Json.Serialization;

namespace ABrechozeiraApp.Models;


public partial class Permission
{
    // ==========================================
    // ENTIDADE: Permission
    // ==========================================
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Column("description")]
    public string? Description { get; set; }

    [Required]
    [MaxLength(50)]
    [Column("resource")]
    public string Resource { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    [Column("action")]
    public string Action { get; set; } = string.Empty;

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navegação - Many-to-Many com Roles
    [JsonIgnore]
    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();

    // Tabela de junção
    [JsonIgnore]
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();

}

