using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using PuntoVenta.Shared.Dtos;

namespace PuntoVenta.Web.Services;

/// <summary>Guarda el JWT y los datos del usuario en el almacenamiento local del navegador
/// (compartido entre pestañas; se borra al cerrar sesion o al expirar el token).</summary>
public class SesionService(ProtectedLocalStorage almacenamiento)
{
    private const string Clave = "sesion_pv";
    private bool _cargada;

    public LoginResponse? Actual { get; private set; }
    public event Action? Cambio;

    public async Task<LoginResponse?> ObtenerAsync()
    {
        if (!_cargada)
        {
            try
            {
                var r = await almacenamiento.GetAsync<LoginResponse>(Clave);
                Actual = r.Success ? r.Value : null;
                _cargada = true;
            }
            catch
            {
                // Durante el prerenderizado no hay JS disponible: se reintenta despues.
                Actual = null;
            }
        }

        if (Actual is not null && Actual.Expira <= DateTime.UtcNow)
            await CerrarAsync();

        return Actual;
    }

    public async Task IniciarAsync(LoginResponse respuesta)
    {
        await almacenamiento.SetAsync(Clave, respuesta);
        Actual = respuesta;
        _cargada = true;
        Cambio?.Invoke();
    }

    public async Task CerrarAsync()
    {
        Actual = null;
        _cargada = true;
        try { await almacenamiento.DeleteAsync(Clave); } catch { /* circuito desconectado */ }
        Cambio?.Invoke();
    }
}