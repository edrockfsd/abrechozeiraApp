using ABrechozeiraApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ABrechozeiraApp.Controllers
{
    // ==========================================
    // CONTROLLER: Roles
    // ==========================================
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class RolesController : ControllerBase
    {
        private readonly AbrechozeiraContext _context;

        public RolesController(AbrechozeiraContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Listar todas as roles com suas permissões
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Role>>> GetRoles()
        {
            return await _context.Role
                .Include(r => r.Permissions)
                .Where(r => r.IsActive)
                .ToListAsync();
        }

        /// <summary>
        /// Buscar role por ID com permissões
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Role>> GetRole(int id)
        {
            var role = await _context.Role
                .Include(r => r.Permissions)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (role == null)
            {
                return NotFound();
            }

            return role;
        }

        /// <summary>
        /// Criar nova role
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Role>> PostRole(Role dto)
        {
            var role = new Role
            {
                Name = dto.Name,
                Description = dto.Description,
                IsActive = dto.IsActive
            };

            _context.Role.Add(role);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetRole", new { id = role.Id }, role);
        }

        /// <summary>
        /// Atualizar role
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRole(int id, Role dto)
        {
            var role = await _context.Role.FindAsync(id);

            if (role == null)
            {
                return NotFound();
            }

            role.Name = dto.Name;
            role.Description = dto.Description;
            role.IsActive = dto.IsActive;
            role.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RoleExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        /// <summary>
        /// Adicionar permissão à role
        /// </summary>
        [HttpPost("{roleId}/permissions/{permissionId}")]
        public async Task<IActionResult> AddPermissionToRole(int roleId, int permissionId)
        {
            var role = await _context.Role
                .Include(r => r.Permissions)
                .FirstOrDefaultAsync(r => r.Id == roleId);

            var permission = await _context.Permission.FindAsync(permissionId);

            if (role == null || permission == null)
            {
                return NotFound();
            }

            if (!role.Permissions.Contains(permission))
            {
                role.Permissions.Add(permission);
                await _context.SaveChangesAsync();
            }

            return NoContent();
        }

        /// <summary>
        /// Remover permissão da role
        /// </summary>
        [HttpDelete("{roleId}/permissions/{permissionId}")]
        public async Task<IActionResult> RemovePermissionFromRole(int roleId, int permissionId)
        {
            var role = await _context.Role
                .Include(r => r.Permissions)
                .FirstOrDefaultAsync(r => r.Id == roleId);

            var permission = await _context.Permission.FindAsync(permissionId);

            if (role == null || permission == null)
            {
                return NotFound();
            }

            if (role.Permissions.Contains(permission))
            {
                role.Permissions.Remove(permission);
                await _context.SaveChangesAsync();
            }

            return NoContent();
        }

        /// <summary>
        /// Deletar role
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(int id)
        {
            var role = await _context.Role.FindAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            role.IsActive = false;
            role.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool RoleExists(int id)
        {
            return _context.Role.Any(e => e.Id == id);
        }
    }
}
