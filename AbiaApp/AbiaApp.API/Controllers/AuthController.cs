using Dapper;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

namespace AbiaApp.API.Controllers
{
    // Modelo para receber os dados do JSON
    public class LoginRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public AuthController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        private string GetConnectionString() => _configuration.GetConnectionString("DefaultConnection");

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            using var conn = new MySqlConnection(GetConnectionString());

            // 1. Busca o usuário pelo Email
            // Nota: Pelas imagens, o nome da tabela parece ser "User" (cuidado com maiúsculas/minúsculas no Linux)
            var sql = "SELECT * FROM User WHERE Email = @Email AND is_active = 1";
            var user = conn.QueryFirstOrDefault(sql, new { request.Email });

            if (user == null)
                return Unauthorized(new { erro = "Usuário ou senha inválidos." });

            // 2. Verifica a senha usando BCrypt
            // A coluna no banco chama 'Password'
            bool senhaValida = BCrypt.Net.BCrypt.Verify(request.Password, user.Password);

            if (!senhaValida)
                return Unauthorized(new { erro = "Usuário ou senha inválidos." });

            // 3. Sucesso! Retorna dados básicos (sem a senha, claro)
            return Ok(new
            {
                mensagem = "Login realizado!",
                usuario = new { user.id, user.Name, user.Email }
            });
        }
    }
}