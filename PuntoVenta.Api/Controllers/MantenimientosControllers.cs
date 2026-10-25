using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PuntoVenta.Api.Data;
using PuntoVenta.Shared.Dtos;
using PuntoVenta.Shared.Models;

namespace PuntoVenta.Api.Controllers;

[Route("api/[controller]")]
public class PuestosController(AppDbContext db) : CrudController<Puesto>(db)
{
    protected override IQueryable<Puesto> Filtrar(IQueryable<Puesto> q, string b) =>
        q.Where(x => x.Nombre.Contains(b) || (x.Descripcion != null && x.Descripcion.Contains(b)));

    protected override async Task<string?> ValidarAsync(Puesto e) =>
        await Db.Puestos.AnyAsync(x => x.Nombre == e.Nombre && x.IdPuesto != e.IdPuesto)
            ? "Ya existe un puesto con ese nombre." : null;
}

[Route("api/[controller]")]
public class MarcasController(AppDbContext db) : CrudController<Marca>(db)
{
    protected override IQueryable<Marca> Filtrar(IQueryable<Marca> q, string b) =>
        q.Where(x => x.Nombre.Contains(b));

    protected override async Task<string?> ValidarAsync(Marca e) =>
        await Db.Marcas.AnyAsync(x => x.Nombre == e.Nombre && x.IdMarca != e.IdMarca)
            ? "Ya existe una marca con ese nombre." : null;
}

[Route("api/[controller]")]
public class ClientesController(AppDbContext db) : CrudController<Cliente>(db)
{
    protected override IQueryable<Cliente> Filtrar(IQueryable<Cliente> q, string b) =>
        q.Where(x => x.Nombres.Contains(b) || x.Apellidos.Contains(b)
            || (x.Nit != null && x.Nit.Contains(b))
            || (x.Correo != null && x.Correo.Contains(b)));

    // NIT vacio se guarda como NULL (la BD permite varios NULL pero no NIT repetidos)
    protected override void Preparar(Cliente e) =>
        e.Nit = string.IsNullOrWhiteSpace(e.Nit) ? null : e.Nit.Trim();

    protected override async Task<string?> ValidarAsync(Cliente e)
    {
        if (e.Nit is null) return null;
        return await Db.Clientes.AnyAsync(x => x.Nit == e.Nit && x.IdCliente != e.IdCliente)
            ? "Ya existe un cliente con ese NIT." : null;
    }
}

[Route("api/[controller]")]
public class ProveedoresController(AppDbContext db) : CrudController<Proveedor>(db)
{
    protected override IQueryable<Proveedor> Filtrar(IQueryable<Proveedor> q, string b) =>
        q.Where(x => x.Nombre.Contains(b) || x.Nit.Contains(b)
            || (x.Correo != null && x.Correo.Contains(b)));

    protected override async Task<string?> ValidarAsync(Proveedor e) =>
        await Db.Proveedores.AnyAsync(x => x.Nit == e.Nit && x.IdProveedor != e.IdProveedor)
            ? "Ya existe un proveedor con ese NIT." : null;
}

[Route("api/[controller]")]
public class EmpleadosController(AppDbContext db) : CrudController<Empleado>(db)
{
    protected override IQueryable<Empleado> Consulta() =>
        Db.Empleados.AsNoTracking().Include(x => x.Puesto);

    protected override IQueryable<Empleado> Filtrar(IQueryable<Empleado> q, string b) =>
        q.Where(x => x.Nombres.Contains(b) || x.Apellidos.Contains(b) || x.Dpi.Contains(b)
            || (x.Correo != null && x.Correo.Contains(b))
            || (x.Puesto != null && x.Puesto.Nombre.Contains(b)));

    protected override void Preparar(Empleado e) => e.Puesto = null;

    protected override async Task<string?> ValidarAsync(Empleado e)
    {
        if (!await Db.Puestos.AnyAsync(p => p.IdPuesto == e.IdPuesto))
            return "El puesto seleccionado no existe.";
        return await Db.Empleados.AnyAsync(x => x.Dpi == e.Dpi && x.IdEmpleado != e.IdEmpleado)
            ? "Ya existe un empleado con ese DPI." : null;
    }
}

[Route("api/[controller]")]
public class ProductosController(AppDbContext db, IWebHostEnvironment env) : CrudController<Producto>(db)
{
    private static readonly string[] Extensiones = [".jpg", ".jpeg", ".png", ".gif", ".webp"];

    protected override IQueryable<Producto> Consulta() =>
        Db.Productos.AsNoTracking().Include(x => x.Marca);

    protected override IQueryable<Producto> Filtrar(IQueryable<Producto> q, string b) =>
        q.Where(x => x.Nombre.Contains(b)
            || (x.Descripcion != null && x.Descripcion.Contains(b))
            || (x.Marca != null && x.Marca.Nombre.Contains(b)));

    protected override void Preparar(Producto e) => e.Marca = null;

    protected override async Task<string?> ValidarAsync(Producto e)
    {
        if (!await Db.Marcas.AnyAsync(m => m.IdMarca == e.IdMarca))
            return "La marca seleccionada no existe.";
        return await Db.Productos.AnyAsync(x => x.Nombre == e.Nombre && x.IdProducto != e.IdProducto)
            ? "Ya existe un producto con ese nombre." : null;
    }

    // Guarda la imagen en el servidor (wwwroot/images/productos) y solo la ruta en la BD.
    [HttpPost("{id:int}/imagen")]
    [RequestSizeLimit(5_000_000)]
    public async Task<IActionResult> SubirImagen(int id, IFormFile archivo)
    {
        var producto = await Db.Productos.FindAsync(id);
        if (producto is null) return NotFound();

        if (archivo is null || archivo.Length == 0)
            return BadRequest(new ErrorDto("Seleccione un archivo de imagen."));

        var ext = Path.GetExtension(archivo.FileName).ToLowerInvariant();
        if (!Extensiones.Contains(ext))
            return BadRequest(new ErrorDto("Formato no valido. Use JPG, PNG, GIF o WEBP."));

        var raiz = env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot");
        var carpeta = Path.Combine(raiz, "images", "productos");
        Directory.CreateDirectory(carpeta);

        var nombre = $"producto_{id}_{Guid.NewGuid():N}{ext}";
        await using (var fs = new FileStream(Path.Combine(carpeta, nombre), FileMode.Create))
            await archivo.CopyToAsync(fs);

        // Borra la imagen anterior, si existía
        if (!string.IsNullOrWhiteSpace(producto.Imagen))
        {
            var anterior = Path.Combine(raiz, producto.Imagen.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
            if (System.IO.File.Exists(anterior)) System.IO.File.Delete(anterior);
        }

        producto.Imagen = $"/images/productos/{nombre}";
        await Db.SaveChangesAsync();
        return Ok(new { imagen = producto.Imagen });
    }
}
