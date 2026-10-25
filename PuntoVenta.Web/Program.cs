using Microsoft.AspNetCore.Components.Authorization;
using PuntoVenta.Web.Components;
using PuntoVenta.Web.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorComponents().AddInteractiveServerComponents();

// Autenticacion basada en el JWT que entrega la API
builder.Services.AddAuthorizationCore();
builder.Services.AddAuthentication("PvCookie")
    .AddCookie("PvCookie", o => o.LoginPath = "/login");
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<SesionService>();
builder.Services.AddScoped<BorradorVenta>();    // venta en curso (no se pierde al cambiar de pantalla)
builder.Services.AddScoped<BorradorCompra>();   // compra en curso
builder.Services.AddScoped<ApiAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<ApiAuthStateProvider>());

// Cliente HTTP hacia la API REST. La URL se toma de appsettings (IP del servidor, no localhost en produccion).
var apiBase = builder.Configuration["Api:BaseUrl"]
    ?? throw new InvalidOperationException("Falta Api:BaseUrl en appsettings.json");
if (!apiBase.EndsWith('/')) apiBase += "/";
builder.Services.AddHttpClient<ApiClient>(c =>
{
    c.BaseAddress = new Uri(apiBase);
    c.Timeout = TimeSpan.FromSeconds(30);
});

var app = builder.Build();

app.UseStaticFiles();
app.UseAntiforgery();
app.MapRazorComponents<App>()
   .AddInteractiveServerRenderMode()
   .AllowAnonymous();   // la autorizacion la hace AuthorizeRouteView con el JWT; la API exige el JWT


app.Run();
