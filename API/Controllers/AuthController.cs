using ABrechozeiraApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ABrechozeiraApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AbrechozeiraContext _context;
        private readonly IConfiguration _configuration;

        public AuthController(AbrechozeiraContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public async Task<ActionResult<object>> Login([FromBody] LoginRequest request)
        {
            // Buscar usuário pelo email
            var user = await _context.User
                .FirstOrDefaultAsync(u => u.Email == request.Email && u.IsActive);

            if (user == null)
            {
                return Unauthorized(new { message = "Usuário não encontrado ou inativo" });
            }

            // Verificar senha
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(request.Password, user.Password);
            if (!isPasswordValid)
            {
                return Unauthorized(new { message = "Credenciais inválidas" });
            }

            // Buscar roles e permissões do usuário
            var userRoles = await _context.UserRole
                .Where(ur => ur.UserId == user.Id)
                .Include(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
                .ToListAsync();

            var roles = userRoles.Select(ur => ur.Role).ToList();
            var permissions = roles
                .SelectMany(r => r.RolePermissions)
                .Select(rp => rp.Permission)
                .Distinct()
                .ToList();

            // Gerar token JWT
            var token = GenerateJwtToken(user, roles, permissions);
            var refreshToken = GenerateRefreshToken();

            // Atualizar o usuário com o refresh token
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                token,
                refreshToken,
                user = new
                {
                    id = user.Id,
                    name = user.Name,
                    email = user.Email,
                    isActive = user.IsActive,
                    roles = roles.Select(r => new
                    {
                        id = r.Id,
                        name = r.Name,
                        description = r.Description,
                        permissions = r.RolePermissions.Select(rp => new
                        {
                            id = rp.Permission.Id,
                            name = rp.Permission.Name,
                            resource = rp.Permission.Resource,
                            action = rp.Permission.Action,
                            description = rp.Permission.Description
                        }).ToList()
                    }).ToList(),
                    permissions = permissions.Select(p => p.Name).ToList()
                }
            });
        }

        [HttpPost("refresh-token")]
        public async Task<ActionResult<object>> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            var principal = GetPrincipalFromExpiredToken(request.Token);
            if (principal == null)
            {
                return BadRequest(new { message = "Token inválido ou expirado" });
            }

            string userIdStr = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdStr, out int userId))
            {
                return BadRequest(new { message = "Token inválido" });
            }

            var user = await _context.User.FindAsync(userId);
            if (user == null || !user.IsActive)
            {
                return Unauthorized(new { message = "Usuário não encontrado ou inativo" });
            }

            // Buscar roles e permissões do usuário
            var userRoles = await _context.UserRole
                .Where(ur => ur.UserId == user.Id)
                .Include(ur => ur.Role)
                    .ThenInclude(r => r.RolePermissions)
                        .ThenInclude(rp => rp.Permission)
                .ToListAsync();

            var roles = userRoles.Select(ur => ur.Role).ToList();
            var permissions = roles
                .SelectMany(r => r.RolePermissions)
                .Select(rp => rp.Permission)
                .Distinct()
                .ToList();

            // Gerar novo token JWT
            var newToken = GenerateJwtToken(user, roles, permissions);
            var newRefreshToken = GenerateRefreshToken();

            // Atualizar o usuário com o novo refresh token
            user.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                token = newToken,
                refreshToken = newRefreshToken
            });
        }

        private string GenerateJwtToken(User user, List<Role> roles, List<Permission> permissions)
        {
            var jwtKey = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey))
            {
                throw new InvalidOperationException("JWT Key não configurada");
            }

            var key = Encoding.ASCII.GetBytes(jwtKey);
            var tokenHandler = new JwtSecurityTokenHandler();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Name)
            };

            // Adicionar roles como claims
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role.Name.ToString()));
            }

            // Adicionar permissões como claims
            foreach (var permission in permissions)
            {
                claims.Add(new Claim("permission", permission.Name));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                ),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"]
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private string GenerateRefreshToken()
        {
            return Guid.NewGuid().ToString();
        }

        private ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var jwtKey = _configuration["Jwt:Key"];
            if (string.IsNullOrEmpty(jwtKey))
            {
                throw new InvalidOperationException("JWT Key não configurada");
            }

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtKey)),
                ValidateIssuer = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = _configuration["Jwt:Audience"],
                ValidateLifetime = false // Não validar expiração para refresh token
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
                if (!(securityToken is JwtSecurityToken jwtSecurityToken) || 
                    !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    return null;
                }

                return principal;
            }
            catch
            {
                return null;
            }
        }
    }

    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class RefreshTokenRequest
    {
        public string Token { get; set; }
        public string RefreshToken { get; set; }
    }
}