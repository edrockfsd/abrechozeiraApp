using ABrechozeiraApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data.SqlTypes;

namespace ABrechozeiraApp.Controllers
{
    // ==========================================
    // CONTROLLER: Users (Atualizado)
    // ==========================================
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class UsersController : ControllerBase
    {
        private readonly AbrechozeiraContext _context;

        public UsersController(AbrechozeiraContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Listar todos os usuários com roles, permissões e dados da pessoa
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetUsers()
        {
            // Buscar usuários com includes
            var users = await _context.User
                .Include(u => u.Roles)
                    .ThenInclude(r => r.Permissions)
                .Include(u => u.Pessoa) // Incluir join com Pessoa
                .Where(u => u.IsActive)
                .ToListAsync();

            // Mapear para o formato desejado (processamento em memória)
            var result = users.Select(u => new
            {
                u.Id,
                u.Email,
                u.Name,
                u.IsActive,
                u.CreatedAt,
                u.UpdatedAt,
                Nome = u.Pessoa.Nome?.Split(' ').FirstOrDefault() ?? string.Empty,
                Sobrenome = string.Join(" ", u.Pessoa.Nome?.Split(' ').Skip(1) ?? Array.Empty<string>()),
                Pessoa = u.Pessoa != null ? new
                {
                    u.Pessoa.Id,
                    
                    u.Pessoa.Email,
                    u.Pessoa.Telefone,
                    u.Pessoa.CPF,
                    u.Pessoa.RG
                } : null,
                Roles = u.Roles.Select(r => new
                {
                    r.Id,
                    r.Name,
                    r.Description,
                    Permissions = r.Permissions.Select(p => new
                    {
                        p.Id,
                        p.Name,
                        p.Description,
                        p.Resource,
                        p.Action
                    })
                }),
                Permissions = u.Roles
                    .SelectMany(r => r.Permissions)
                    .Select(p => p.Name)
                    .Distinct()
                    .ToList()
            }).ToList();

            return Ok(result);
        }

        [HttpGet("GetUsersOptimized")]
        public async Task<ActionResult<IEnumerable<object>>> GetUsersOptimized()
        {
            var sql = @"
                        SELECT 
                            u.id, u.email, u.name, u.is_active, u.created_at, u.updated_at,
                            r.id as role_id, r.name as role_name, r.description as role_description,
                            p.id as permission_id, p.name as permission_name, 
                            p.description as permission_description, p.resource, p.action
                        FROM users u
                        LEFT JOIN user_roles ur ON u.id = ur.user_id
                        LEFT JOIN roles r ON ur.role_id = r.id
                        LEFT JOIN role_permissions rp ON r.id = rp.role_id
                        LEFT JOIN permissions p ON rp.permission_id = p.id
                        WHERE u.is_active = 1
                        ORDER BY u.id, r.id, p.id";

            var result = await _context.Database.SqlQueryRaw<dynamic>(sql).ToListAsync();

            return Ok(result);
        }

        /// <summary>
        /// Buscar usuário por ID com roles, permissões e dados da pessoa
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetUser(int id)
        {// Buscar usuário com includes (query simples no banco)
            var user = await _context.User
                .Include(u => u.Roles)
                    .ThenInclude(r => r.Permissions)
                .Include(u => u.Pessoa) // Incluir join com Pessoa
                .FirstOrDefaultAsync(u => u.Id == id && u.IsActive);

            if (user == null)
            {
                return NotFound();
            }

            // Mapear para o formato desejado (processamento em memória)
            var result = new
            {
                user.Id,
                user.Email,
                user.Name,
                user.IsActive,
                user.CreatedAt,
                user.UpdatedAt,
                Nome = user.Pessoa.Nome?.Split(' ').FirstOrDefault() ?? string.Empty,
                Sobrenome = string.Join(" ", user.Pessoa.Nome?.Split(' ').Skip(1) ?? Array.Empty<string>()),
                Pessoa = user.Pessoa != null ? new
                {
                    user.Pessoa.Id,
                    
                    
                    user.Pessoa.Email,
                    user.Pessoa.Telefone,
                    user.Pessoa.CPF,
                    user.Pessoa.RG
                } : null,
                Roles = user.Roles.Where(r => r.IsActive).Select(r => new
                {
                    r.Id,
                    r.Name,
                    r.Description,
                    Permissions = r.Permissions.Where(p => p.IsActive).Select(p => new
                    {
                        p.Id,
                        p.Name,
                        p.Description,
                        p.Resource,
                        p.Action
                    }).ToList()
                }).ToList(),
                Permissions = user.Roles
                    .Where(r => r.IsActive)
                    .SelectMany(r => r.Permissions.Where(p => p.IsActive))
                    .Select(p => p.Name)
                    .Distinct()
                    .ToList()
            };

            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<User>> PostUser(CreateUserRequest dto)
        {
            // Check if the Pessoa already has a user
            var pessoa = await _context.Pessoa.Include(p => p.User).FirstOrDefaultAsync(p => p.Id == dto.PessoaId);
            if (pessoa == null)
            {
                return BadRequest("Pessoa não encontrada.");
            }

            if (pessoa.User != null)
            {
                return BadRequest("Esta pessoa já está associada a um usuário.");
            }

            var user = new User
            {
                Email = dto.Email,
                Name = dto.Name,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password), // Hash da senha
                IsActive = dto.IsActive,
                PessoaId = dto.PessoaId
            };
        
            _context.User.Add(user);
            await _context.SaveChangesAsync();

            // Recarregar com includes mínimos para montar DTO e evitar ciclos
            var created = await _context.User
                .Include(u => u.Roles)
                .ThenInclude(r => r.Permissions)
                .Include(u => u.Pessoa)
                .FirstOrDefaultAsync(u => u.Id == user.Id);

            if (created == null)
            {
                return CreatedAtAction("GetUser", new { id = user.Id }, null);
            }

            var result = new
            {
                created.Id,
                created.Email,
                created.Name,
                created.IsActive,
                created.CreatedAt,
                created.UpdatedAt,
                Nome = created.Pessoa?.Nome?.Split(' ').FirstOrDefault() ?? string.Empty,
                Sobrenome = string.Join(" ", created.Pessoa?.Nome?.Split(' ').Skip(1) ?? Array.Empty<string>()),
                Pessoa = created.Pessoa != null ? new
                {
                    created.Pessoa.Id,
                    created.Pessoa.Email,
                    created.Pessoa.Telefone,
                    created.Pessoa.CPF,
                    created.Pessoa.RG
                } : null,
                Roles = created.Roles.Select(r => new
                {
                    r.Id,
                    r.Name,
                    r.Description,
                    Permissions = r.Permissions.Select(p => new
                    {
                        p.Id,
                        p.Name,
                        p.Description,
                        p.Resource,
                        p.Action
                    })
                })
            };

            return CreatedAtAction("GetUser", new { id = created.Id }, result);
        }

        /// <summary>
        /// Atualizar usuário
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUser(int id, UpdateUserRequest dto)
        {
            var user = await _context.User.FindAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            if (!string.IsNullOrWhiteSpace(dto.Email))
            {
                user.Email = dto.Email;
            }

            if (!string.IsNullOrWhiteSpace(dto.Name))
            {
                user.Name = dto.Name;
            }

            if (dto.IsActive.HasValue)
            {
                user.IsActive = dto.IsActive.Value;
            }
            user.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(dto.Password))
            {
                user.Password = BCrypt.Net.BCrypt.HashPassword(dto.Password);
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        /// <summary>
        /// Adicionar role ao usuário
        /// </summary>
        [HttpPost("{userId}/roles/{roleId}")]
        public async Task<IActionResult> AddRoleToUser(int userId, int roleId)
        {
            var user = await _context.User
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Id == userId);

            var role = await _context.Role.FindAsync(roleId);

            if (user == null || role == null)
            {
                return NotFound();
            }

            if (!user.Roles.Contains(role))
            {
                user.Roles.Add(role);
                await _context.SaveChangesAsync();
            }

            return NoContent();
        }

        /// <summary>
        /// Remover role do usuário
        /// </summary>
        [HttpDelete("{userId}/roles/{roleId}")]
        public async Task<IActionResult> RemoveRoleFromUser(int userId, int roleId)
        {
            var user = await _context.User
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Id == userId);

            var role = await _context.Role.FindAsync(roleId);

            if (user == null || role == null)
            {
                return NotFound();
            }

            if (user.Roles.Contains(role))
            {
                user.Roles.Remove(role);
                await _context.SaveChangesAsync();
            }

            return NoContent();
        }

        /// <summary>
        /// Deletar usuário
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.User.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserExists(int id)
        {
            return _context.User.Any(e => e.Id == id);
        }
    }
}


public class CreateUserRequest
{
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public int PessoaId { get; set; }
}

public class UpdateUserRequest
{
    public string? Email { get; set; }
    public string? Name { get; set; }
    public string? Password { get; set; }
    public bool? IsActive { get; set; }
}

