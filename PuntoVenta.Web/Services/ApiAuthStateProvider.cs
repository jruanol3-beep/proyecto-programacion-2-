using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace PuntoVenta.Web.Services;

public class ApiAuthStateProvider : AuthenticationStateProvider, IDisposable
{
    private readonly SesionService _sesion;

    public ApiAuthStateProvider(SesionService sesion)
    {
        _sesion = sesion;
        _sesion.Cambio += Notificar;
    }

    private void Notificar() => NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var s = await _sesion.ObtenerAsync();
        if (s is null)
            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));

        var identidad = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, s.Usuario),
            new Claim("nombre_completo", s.NombreCompleto),
            new Claim(ClaimTypes.Role, s.Rol)
        }, authenticationType: "jwt");

        return new AuthenticationState(new ClaimsPrincipal(identidad));
    }

    public void Dispose() => _sesion.Cambio -= Notificar;
}
