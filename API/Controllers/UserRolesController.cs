using ABrechozeiraApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ABrechozeiraApp.Controllers
{
    // ==========================================
    // CONTROLLER: UserRoles
    // ==========================================
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class UserRolesController : ControllerBase
    {
        private readonly AbrechozeiraContext _context;

        public UserRolesController(AbrechozeiraContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Listar todas as atribuições de papéis de usuário
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserRole>>> GetUserRoles()
        {
            return await _context.UserRole
                                 .Include(ur => ur.User)
                                 .Include(ur => ur.Role)
                                 .ToListAsync();
        }

        /// <summary>
        /// Buscar atribuição por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<UserRole>> GetUserRole(int id)
        {
            var userRole = await _context.UserRole
                                         .Include(ur => ur.User)
                                         .Include(ur => ur.Role)
                                         .FirstOrDefaultAsync(ur => ur.Id == id);

            if (userRole == null)
            {
                return NotFound();
            }

            return userRole;
        }

        /// <summary>
        /// Atribuir um papel a um usuário
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<UserRole>> PostUserRole(UserRole userRole)
        {
            // Verifica se a combinação de usuário e papel já existe
            var existingUserRole = await _context.UserRole
                .FirstOrDefaultAsync(ur => ur.UserId == userRole.UserId && ur.RoleId == userRole.RoleId);

            if (existingUserRole != null)
            {
                return Conflict("This user is already assigned to this role.");
            }

            _context.UserRole.Add(userRole);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUserRole", new { id = userRole.Id }, userRole);
        }

        /// <summary>
        /// Atualizar uma atribuição de papel
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUserRole(int id, UserRole userRole)
        {
            if (id != userRole.Id)
            {
                return BadRequest();
            }

            _context.Entry(userRole).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserRoleExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        /// <summary>
        /// Deletar uma atribuição de papel
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUserRole(int id)
        {
            var userRole = await _context.UserRole.FindAsync(id);
            if (userRole == null)
            {
                return NotFound();
            }

            _context.UserRole.Remove(userRole);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool UserRoleExists(int id)
        {
            return _context.UserRole.Any(e => e.Id == id);
        }
    }
}
