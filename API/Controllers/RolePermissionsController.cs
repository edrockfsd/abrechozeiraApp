using ABrechozeiraApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ABrechozeiraApp.Controllers
{
    // ==========================================
    // CONTROLLER: RolePermissions
    // ==========================================
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class RolePermissionsController : ControllerBase
    {
        private readonly AbrechozeiraContext _context;

        public RolePermissionsController(AbrechozeiraContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Listar todas as permissões
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RolePermission>>> GetRolePermissions()
        {
            return await _context.RolePermission                
                .ToListAsync();
        }

        /// <summary>
        /// Buscar permissão por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<RolePermission>> GetRolePermission(int id)
        {
            var RolePermission = await _context.RolePermission.FindAsync(id);

            if (RolePermission == null)
            {
                return NotFound();
            }

            return RolePermission;
        }

        /// <summary>
        /// Criar nova permissão
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<RolePermission>> PostRolePermission(RolePermission dto)
        {
            var RolePermission = new RolePermission
            {
                RoleId = dto.RoleId,
                PermissionId = dto.PermissionId,
                CreatedAt = dto.CreatedAt
            };

            _context.RolePermission.Add(RolePermission);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetRolePermission", new { id = RolePermission.Id }, RolePermission);
        }

        /// <summary>
        /// Atualizar permissão
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutRolePermission(int id, RolePermission dto)
        {
            var RolePermission = await _context.RolePermission.FindAsync(id);

            if (RolePermission == null)
            {
                return NotFound();
            }

            RolePermission.RoleId = dto.RoleId;
            RolePermission.PermissionId = dto.PermissionId;
            RolePermission.CreatedAt = dto.CreatedAt;
            

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!RolePermissionExists(id))
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
        public async Task<IActionResult> DeleteRolePermission(int id)
        {
            var RolePermission = await _context.RolePermission.FindAsync(id);
            if (RolePermission == null)
            {
                return NotFound();
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool RolePermissionExists(int id)
        {
            return _context.RolePermission.Any(e => e.Id == id);
        }
    }
}
