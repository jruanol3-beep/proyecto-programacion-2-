using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace PuntoVenta.Web.Services;

public class ApiException(string mensaje, HttpStatusCode? codigo = null) : Exception(mensaje)
{
    public HttpStatusCode? Codigo { get; } = codigo;
}

/// <summary>Cliente de la API REST: agrega el JWT a cada solicitud y traduce los errores a mensajes legibles.</summary>
public class ApiClient
{
    private static readonly JsonSerializerOptions Opciones = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _http;
    private readonly SesionService _sesion;
    private readonly string _urlPublica;

    public ApiClient(HttpClient http, SesionService sesion, IConfiguration config)
    {
        _http = http;
        _sesion = sesion;
        _urlPublica = (config["Api:PublicUrl"] ?? config["Api:BaseUrl"] ?? "").TrimEnd('/');
    }

    /// <summary>URL absoluta (alcanzable desde el navegador) de una imagen guardada por la API.</summary>
    public string? UrlImagen(string? ruta) =>
        string.IsNullOrWhiteSpace(ruta) ? null : _urlPublica + ruta;

    public async Task<T> GetAsync<T>(string url)
    {
        using var resp = await EnviarAsync(HttpMethod.Get, url);
        var datos = await resp.Content.ReadFromJsonAsync<T>(Opciones);
        return datos ?? throw new ApiException("El servidor devolvio una respuesta vacia.");
    }

    public async Task<TResp> PostAsync<TResp>(string url, object cuerpo)
    {
        using var contenido = JsonContent.Create(cuerpo, cuerpo.GetType(), options: Opciones);
        using var resp = await EnviarAsync(HttpMethod.Post, url, contenido);
        var datos = await resp.Content.ReadFromJsonAsync<TResp>(Opciones);
        return datos ?? throw new ApiException("El servidor devolvio una respuesta vacia.");
    }

    public async Task PutAsync(string url, object cuerpo)
    {
        using var contenido = JsonContent.Create(cuerpo, cuerpo.GetType(), options: Opciones);
        using var resp = await EnviarAsync(HttpMethod.Put, url, contenido);
    }

    public async Task DeleteAsync(string url)
    {
        using var resp = await EnviarAsync(HttpMethod.Delete, url);
    }

    public async Task SubirImagenAsync(int idProducto, Stream flujo, string nombre, string tipo)
    {
        using var contenido = new MultipartFormDataContent();
        var archivo = new StreamContent(flujo);
        archivo.Headers.ContentType = new MediaTypeHeaderValue(
            string.IsNullOrWhiteSpace(tipo) ? "application/octet-stream" : tipo);
        contenido.Add(archivo, "archivo", nombre);
        using var resp = await EnviarAsync(HttpMethod.Post, $"api/Productos/{idProducto}/imagen", contenido);
    }

    public static string Buscar(string url, string? texto) =>
        string.IsNullOrWhiteSpace(texto) ? url : $"{url}?buscar={Uri.EscapeDataString(texto.Trim())}";

    private async Task<HttpResponseMessage> EnviarAsync(HttpMethod metodo, string url, HttpContent? contenido = null)
    {
        var sesion = await _sesion.ObtenerAsync();

        using var solicitud = new HttpRequestMessage(metodo, url) { Content = contenido };
        if (sesion is not null)
            solicitud.Headers.Authorization = new AuthenticationHeaderValue("Bearer", sesion.Token);

        HttpResponseMessage resp;
        try
        {
            resp = await _http.SendAsync(solicitud);
        }
        catch (HttpRequestException)
        {
            throw new ApiException("No se pudo conectar con el servidor de la API. Verifique que este en linea.");
        }
        catch (TaskCanceledException)
        {
            throw new ApiException("El servidor tardo demasiado en responder.");
        }

        if (resp.IsSuccessStatusCode) return resp;

        var mensaje = await LeerErrorAsync(resp);
        var codigo = resp.StatusCode;
        resp.Dispose();

        if (codigo == HttpStatusCode.Unauthorized && sesion is not null)
        {
            await _sesion.CerrarAsync();
            throw new ApiException("Su sesion expiro. Inicie sesion nuevamente.", codigo);
        }
        throw new ApiException(mensaje, codigo);
    }

    private static async Task<string> LeerErrorAsync(HttpResponseMessage resp)
    {
        var texto = await resp.Content.ReadAsStringAsync();
        try
        {
            using var doc = JsonDocument.Parse(texto);
            var raiz = doc.RootElement;
            if (raiz.ValueKind == JsonValueKind.Object)
            {
                // Formato propio: { "mensaje": "..." }
                if (raiz.TryGetProperty("mensaje", out var m) && m.ValueKind == JsonValueKind.String)
                    return m.GetString() ?? "Error desconocido.";

                // Validaciones automaticas de ASP.NET Core: { "errors": { "Campo": ["msg"] } }
                if (raiz.TryGetProperty("errors", out var errores) && errores.ValueKind == JsonValueKind.Object)
                {
                    var mensajes = errores.EnumerateObject()
                        .SelectMany(p => p.Value.ValueKind == JsonValueKind.Array
                            ? p.Value.EnumerateArray().Select(x => x.GetString())
                            : Enumerable.Empty<string?>())
                        .Where(x => !string.IsNullOrWhiteSpace(x));
                    var unidos = string.Join(" ", mensajes);
                    if (unidos.Length > 0) return unidos;
                }

                if (raiz.TryGetProperty("title", out var t) && t.ValueKind == JsonValueKind.String)
                    return t.GetString() ?? "Error desconocido.";
            }
        }
        catch (JsonException) { /* cuerpo no es JSON */ }

        return resp.StatusCode switch
        {
            HttpStatusCode.Unauthorized => "No autorizado.",
            HttpStatusCode.Forbidden => "No tiene permisos para esta accion.",
            HttpStatusCode.NotFound => "Recurso no encontrado.",
            _ => $"Error del servidor ({(int)resp.StatusCode})."
        };
    }
}
