using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace ABrechozeiraApp.Models;

[Table("User")]
public partial class User
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    [Column("Name")]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    [Column("Email")]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    [JsonIgnore]
    [Column("Password")]
    public string Password { get; set; } = string.Empty;

    [Column("is_active")]
    public bool IsActive { get; set; } = true;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    // Vinculo obrigatorio com Pessoa
    [Required]
    [Column("PessoaId")]
    public int PessoaId { get; set; }

    [ForeignKey("PessoaId")]
    [JsonIgnore]
    public virtual Pessoa? Pessoa { get; set; }

    // Relacionamentos com Roles
    [JsonIgnore]
    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();

    [JsonIgnore]
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();

    // Propriedade calculada para retornar lista de permissoes
    [NotMapped]
    public List<string> Permissions => Roles
        .SelectMany(r => r.Permissions)
        .Select(p => p.Name)
        .Distinct()
        .ToList();
}
