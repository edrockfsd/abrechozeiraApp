using ABrechozeiraApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ABrechozeiraApp.Controllers
{
    // ==========================================
    // CONTROLLER: Permissions
    // ==========================================
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class PermissionsController : ControllerBase
    {
        private readonly AbrechozeiraContext _context;

        public PermissionsController(AbrechozeiraContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Listar todas as permissões
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Permission>>> GetPermissions()
        {
            return await _context.Permission
                .Where(p => p.IsActive)
                .ToListAsync();
        }

        /// <summary>
        /// Buscar permissão por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<Permission>> GetPermission(int id)
        {
            var permission = await _context.Permission.FindAsync(id);

            if (permission == null)
            {
                return NotFound();
            }

            return permission;
        }

        /// <summary>
        /// Criar nova permissão
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<Permission>> PostPermission(Permission dto)
        {
            var permission = new Permission
            {
                Name = dto.Name,
                Description = dto.Description,
                Resource = dto.Resource,
                Action = dto.Action,
                IsActive = dto.IsActive
            };

            _context.Permission.Add(permission);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPermission", new { id = permission.Id }, permission);
        }

        /// <summary>
        /// Atualizar permissão
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPermission(int id, Permission dto)
        {
            var permission = await _context.Permission.FindAsync(id);

            if (permission == null)
            {
                return NotFound();
            }

            permission.Name = dto.Name;
            permission.Description = dto.Description;
            permission.Resource = dto.Resource;
            permission.Action = dto.Action;
            permission.IsActive = dto.IsActive;
            permission.UpdatedAt = DateTime.UtcNow;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PermissionExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        /// <summary>
        /// Deletar permissão
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePermission(int id)
        {
            var permission = await _context.Permission.FindAsync(id);
            if (permission == null)
            {
                return NotFound();
            }

            permission.IsActive = false;
            permission.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PermissionExists(int id)
        {
            return _context.Permission.Any(e => e.Id == id);
        }
    }
}
