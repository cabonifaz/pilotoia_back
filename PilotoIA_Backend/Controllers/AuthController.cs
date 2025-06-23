using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using PilotoIA_Backend.Models;
using PilotoIA_Backend.DataAccess;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using PilotoIA_Backend.BusinessLogic;
using Microsoft.Extensions.Options;

namespace PilotoIA_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AuthHandler _authHandler;
        private readonly beMySettings vgSettings;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IOptions<beMySettings> peSettings, ILogger<AuthController> logger)
        {
            vgSettings = peSettings.Value;
            _logger = logger;
            _authHandler = new AuthHandler(vgSettings);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest loginRequest)
        {
            if (loginRequest == null || string.IsNullOrEmpty(loginRequest.Username) || string.IsNullOrEmpty(loginRequest.Password))
            {
                return BadRequest(new LoginResponse
                {
                    Result = new MensajeRespuesta
                    {
                        Mensaje = "Usuario y contraseña son requeridos.",
                        IdTipoMensaje = 1
                    }
                });
            }

            try
            {
                // Validar credenciales con la base de datos
                var resultado = await _authHandler.ValidarCredencialesAsync(loginRequest.Username, loginRequest.Password);

                if (resultado.IdTipoMensaje != 2)
                {
                    return Unauthorized(new LoginResponse
                    {
                        Result = resultado
                    });
                }

                // Generar token JWT
                var tokenInfo = _authHandler.GenerateToken();

                return Ok(new LoginResponse
                {
                    Token = tokenInfo,
                    Result = resultado
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error durante el login");
                return StatusCode(500, new LoginResponse
                {
                    Result = new MensajeRespuesta
                    {
                        IdMensaje = -1,
                        Mensaje = "Error interno del servidor",
                        IdTipoMensaje = 0
                    }
                });
            }
        }

        [HttpGet("validar-token")]
        public IActionResult ValidarToken([FromHeader(Name = "Authorization")] string authHeader)
        {
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                return Unauthorized(new ValidateTokenResponse
                {
                    Result = new MensajeRespuesta
                    {
                        Mensaje = "Token de autorización no proporcionado o inválido.",
                        IdTipoMensaje = 1
                    }
                });
            }

            var token = authHeader.Substring("Bearer ".Length).Trim();
            var resultado = _authHandler.ValidarToken(token);

            if (resultado.IdTipoMensaje == 2)
            {
                return Ok(new ValidateTokenResponse
                {
                    IsValid = true,
                    Result = resultado
                }
                );
            }

            return Unauthorized(resultado);
        }
    }
}