using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using PuntoVenta.Api.Models;

namespace PuntoVenta.Api.Services;

public class TokenService(IConfiguration config)
{
    public (string Token, DateTime Expira) Crear(Usuario usuario, string rol)
    {
        var jwt = config.GetSection("Jwt");
        var minutos = int.TryParse(jwt["ExpireMinutes"], out var m) ? m : 120;
        var expira = DateTime.UtcNow.AddMinutes(minutos);

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim("sub", usuario.IdUsuario.ToString()),
                new Claim("name", usuario.NombreUsuario),
                new Claim("nombre_completo", usuario.NombreCompleto),
                new Claim("role", rol)
            }),
            Issuer = jwt["Issuer"],
            Audience = jwt["Audience"],
            Expires = expira,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt["Key"]!)),
                SecurityAlgorithms.HmacSha256)
        };

        return (new JsonWebTokenHandler().CreateToken(descriptor), expira);
    }
}
