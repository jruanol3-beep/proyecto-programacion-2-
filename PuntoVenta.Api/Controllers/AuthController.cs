using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PuntoVenta.Api.Data;
using PuntoVenta.Api.Services;
using PuntoVenta.Shared.Dtos;

namespace PuntoVenta.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(AppDbContext db, TokenService tokens) : ControllerBase
{
    /// <summary>Valida usuario y contrasena y devuelve un JWT.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        var usuario = await db.Usuarios.Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.NombreUsuario == request.Usuario && u.Activo);

        if (usuario is null || !BCrypt.Net.BCrypt.Verify(request.Password, usuario.PasswordHash))
            return Unauthorized(new ErrorDto("Usuario o contrasena incorrectos."));

        var rol = usuario.Rol?.Nombre ?? "Usuario";
        var (token, expira) = tokens.Crear(usuario, rol);

        return new LoginResponse
        {
            Token = token,
            Usuario = usuario.NombreUsuario,
            NombreCompleto = usuario.NombreCompleto,
            Rol = rol,
            Expira = expira
        };
    }
}
