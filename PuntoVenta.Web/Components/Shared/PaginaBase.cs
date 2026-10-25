using Microsoft.AspNetCore.Components;
using PuntoVenta.Web.Services;

namespace PuntoVenta.Web.Components.Shared;

/// <summary>Base comun de las paginas: acceso a la API y manejo uniforme de mensajes y errores.</summary>
public abstract class PaginaBase : ComponentBase
{
    [Inject] protected ApiClient Api { get; set; } = default!;
    [Inject] protected NavigationManager Nav { get; set; } = default!;

    protected string? Mensaje { get; set; }
    protected string TipoMensaje { get; set; } = "success";
    protected bool Cargando { get; set; }

    protected void Exito(string texto) { Mensaje = texto; TipoMensaje = "success"; }
    protected void Fallo(string texto) { Mensaje = texto; TipoMensaje = "danger"; }
    protected void Aviso(string texto) { Mensaje = texto; TipoMensaje = "warning"; }
    protected void CerrarMensaje() => Mensaje = null;

    /// <summary>Copia profunda simple para editar un registro sin modificar la lista mostrada.</summary>
    protected static T Clonar<T>(T original) =>
        System.Text.Json.JsonSerializer.Deserialize<T>(System.Text.Json.JsonSerializer.Serialize(original))!;

    /// <summary>Ejecuta una accion contra la API; si falla, muestra el mensaje de error. Devuelve true si salio bien.</summary>
    protected async Task<bool> Ejecutar(Func<Task> accion, string? mensajeExito = null)
    {
        try
        {
            await accion();
            if (mensajeExito is not null) Exito(mensajeExito);
            return true;
        }
        catch (ApiException ex)
        {
            Fallo(ex.Message);
            return false;
        }
    }
}
