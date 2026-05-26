using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ABrechozeiraApp.Models;

public enum RoleType
{
    ADMIN,
    MANAGER,
    SELLER,
    VIEWER
}

public partial class Role
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [Column("name")]
    public RoleType Name { get; set; }

    [Column("description")]
    public string? Description { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navegação - Many-to-Many com Users
    [JsonIgnore]
    public virtual ICollection<User> Users { get; set; } = new List<User>();

    // Navegação - Many-to-Many com Permissions
    public virtual ICollection<Permission> Permissions { get; set; } = new List<Permission>();

    // Tabelas de junção
    [JsonIgnore]
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    [JsonIgnore]
    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}

