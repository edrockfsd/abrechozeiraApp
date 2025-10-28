using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ABrechozeiraApp.Models;

public partial class User
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    // Foreign key to Pessoa
    [Column("PessoaId")]
    public int PessoaId { get; set; }

    [ForeignKey("PessoaId")]
    public virtual Pessoa? Pessoa { get; set; }

    [Required]
    [MaxLength(255)]
    [Column("email")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    [Column("name")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [MaxLength(255)]
    [Column("password")]
    [JsonIgnore] // Nunca retornar senha na API
    public string Password { get; set; } = string.Empty;

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navegação - Many-to-Many com Roles
    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();

    // Tabela de junção
    [JsonIgnore]
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    // Propriedade calculada para retornar lista de permissões (para API)
    [NotMapped]
    public List<string> Permissions =>
        Roles.SelectMany(r => r.Permissions.Select(p => p.Name)).Distinct().ToList();
}