using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PuntoVenta.Api.Data;
using PuntoVenta.Api.Models;
using PuntoVenta.Api.Services;
using PuntoVenta.Shared.Dtos;

var builder = WebApplication.CreateBuilder(args);

// ---- Base de datos (MySQL + Entity Framework Core) ----
var connectionString = builder.Configuration.GetConnectionString("PuntoVenta")
    ?? throw new InvalidOperationException("Falta la cadena de conexion 'PuntoVenta'.");
builder.Services.AddDbContext<AppDbContext>(options => options.UseMySQL(connectionString));

// ---- JWT ----
var jwt = builder.Configuration.GetSection("Jwt");
var key = jwt["Key"] ?? throw new InvalidOperationException("Falta Jwt:Key.");
builder.Services.AddSingleton<TokenService>();
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt["Issuer"],
            ValidAudience = jwt["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
            NameClaimType = "name",
            RoleClaimType = "role",
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });
builder.Services.AddAuthorization();

// ---- Controladores ----
builder.Services.AddControllers();

var app = builder.Build();

// Crea el usuario administrador la primera vez (contrasena Admin123*)
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        if (!await db.Usuarios.AnyAsync())
        {
            var rolAdmin = await db.Roles.FirstOrDefaultAsync(r => r.Nombre == "Administrador");
            if (rolAdmin is null)
            {
                rolAdmin = new Rol { Nombre = "Administrador" };
                db.Roles.Add(rolAdmin);
                await db.SaveChangesAsync();
            }
            db.Usuarios.Add(new Usuario
            {
                NombreUsuario = "admin",
                NombreCompleto = "Administrador del sistema",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123*"),
                IdRol = rolAdmin.IdRol
            });
            await db.SaveChangesAsync();
            logger.LogInformation("Usuario 'admin' creado.");
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "No se pudo inicializar el usuario administrador. Verifique la base de datos.");
    }
}

// Respuesta uniforme ante errores no controlados
app.UseExceptionHandler(errorApp => errorApp.Run(async context =>
{
    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
    await context.Response.WriteAsJsonAsync(new ErrorDto("Ocurrio un error inesperado en el servidor."));
}));

app.UseStaticFiles();   // sirve /images/productos/...
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
