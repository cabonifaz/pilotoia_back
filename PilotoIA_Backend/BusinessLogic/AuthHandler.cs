using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using PilotoIA_Backend.DataAccess;
using PilotoIA_Backend.Models;

namespace PilotoIA_Backend.BusinessLogic
{
    public class AuthHandler
    {
        private readonly AuthDAO vgDataAccess;
        private readonly beMySettings vgSettings;

        public AuthHandler(beMySettings peSettings)
        {
            vgSettings = peSettings;
            vgDataAccess = new AuthDAO(vgSettings.DbConnection);
        }

        public async Task<MensajeRespuesta> ValidarCredencialesAsync(string username, string password)
        {
            return await vgDataAccess.ValidarCredenciales(username, password);
        }

        public string GenerateToken()
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(vgSettings.Jwt.Secret));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
                }),
                Expires = DateTime.UtcNow.AddMinutes(vgSettings.Jwt.ExpireMinutes),
                SigningCredentials = credentials,
                Issuer = vgSettings.Jwt.Issuer,
                Audience = vgSettings.Jwt.Audience
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public MensajeRespuesta ValidarToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var key = Encoding.ASCII.GetBytes(vgSettings.Jwt.Secret);

                // Configuración de validación
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(key),
                    ValidateIssuer = true,
                    ValidIssuer = vgSettings.Jwt.Issuer,
                    ValidateAudience = true,
                    ValidAudience = vgSettings.Jwt.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero // Sin margen de tiempo para la expiración
                };

                // Validación del token
                tokenHandler.ValidateToken(token, validationParameters, out _);

                return new MensajeRespuesta
                {
                    IdTipoMensaje = 2,
                    Mensaje = "Token válido",
                    IdMensaje = 200
                };
            }
            catch (SecurityTokenExpiredException)
            {
                return new MensajeRespuesta
                {
                    IdTipoMensaje = 1,
                    Mensaje = "Token expirado",
                    IdMensaje = 401
                };
            }
            catch (SecurityTokenInvalidSignatureException)
            {
                return new MensajeRespuesta
                {
                    IdTipoMensaje = 1,
                    Mensaje = "Firma del token no válida",
                    IdMensaje = 401
                };
            }
            catch (SecurityTokenInvalidIssuerException)
            {
                return new MensajeRespuesta
                {
                    IdTipoMensaje = 1,
                    Mensaje = "Issuer del token no válido",
                    IdMensaje = 401
                };
            }
            catch (SecurityTokenInvalidAudienceException)
            {
                return new MensajeRespuesta
                {
                    IdTipoMensaje = 1,
                    Mensaje = "Audience del token no válido",
                    IdMensaje = 401
                };
            }
            catch (Exception ex)
            {
                return new MensajeRespuesta
                {
                    IdTipoMensaje = 1,
                    Mensaje = $"Token no válido: {ex.Message}",
                    IdMensaje = 401
                };
            }
        }
    }
}