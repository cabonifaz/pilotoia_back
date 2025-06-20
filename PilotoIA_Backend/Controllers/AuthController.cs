using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using PilotoIA_Backend.Models;
using PilotoIA_Backend.DataAccess;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PilotoIA_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthController> _logger;
        private readonly AuthDAO _authDAO;

        public AuthController(IConfiguration configuration, ILogger<AuthController> logger)
        {
            _configuration = configuration;
            _logger = logger;
            _authDAO = new AuthDAO(configuration.GetConnectionString("DefaultConnection"));
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            if (loginRequest == null || string.IsNullOrEmpty(loginRequest.Username) || string.IsNullOrEmpty(loginRequest.Password))
            {
                return BadRequest(new LoginResponse
                {
                    Mensaje = new MensajeRespuesta
                    {
                        IdMensaje = 0,
                        Mensaje = "Usuario y contraseña son requeridos.",
                        IdTipoMensaje = 0 // 0 para errores
                    }
                });
            }

            try
            {
                // Validar credenciales con la base de datos
                var resultado = await _authDAO.ValidarCredenciales(loginRequest.Username, loginRequest.Password);

                if (resultado.IdTipoMensaje != 1) // Asumiendo que 1 es éxito
                {
                    return Unauthorized(new LoginResponse
                    {
                        Mensaje = resultado
                    });
                }

                // Generar token JWT
                var tokenInfo = GenerateToken(loginRequest.Username);

                return Ok(new LoginResponse
                {
                    Token = tokenInfo.Token,
                    Mensaje = resultado
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante el login");
                return StatusCode(500, new LoginResponse
                {
                    Mensaje = new MensajeRespuesta
                    {
                        IdMensaje = -1,
                        Mensaje = "Error interno del servidor",
                        IdTipoMensaje = 0
                    }
                });
            }
        }

        private (string Token, DateTime Expiration) GenerateToken(string username)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpireMinutes"])),
                signingCredentials: credentials);

            return (new JwtSecurityTokenHandler().WriteToken(token), token.ValidTo);
        }
    }
}