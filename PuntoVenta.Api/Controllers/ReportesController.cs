using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PuntoVenta.Api.Data;
using PuntoVenta.Shared.Dtos;

namespace PuntoVenta.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ReportesController(AppDbContext db) : ControllerBase
{
    [HttpGet("productos")]
    public async Task<ActionResult<List<ReporteProductoDto>>> Productos()
    {
        var lista = await db.Productos.AsNoTracking().Include(p => p.Marca)
            .OrderBy(p => p.Nombre).ToListAsync();
        return lista.Select(p => new ReporteProductoDto
        {
            Producto = p.Nombre,
            Marca = p.Marca?.Nombre ?? "",
            Existencia = p.Existencia,
            PrecioCosto = p.PrecioCosto,
            PrecioVenta = p.PrecioVenta
        }).ToList();
    }

    [HttpGet("existencias")]
    public async Task<ActionResult<List<ReporteExistenciaDto>>> Existencias()
    {
        var lista = await db.Productos.AsNoTracking().Include(p => p.Marca)
            .OrderBy(p => p.Existencia).ThenBy(p => p.Nombre).ToListAsync();
        return lista.Select(p => new ReporteExistenciaDto
        {
            Producto = p.Nombre,
            Marca = p.Marca?.Nombre ?? "",
            Existencia = p.Existencia,
            ValorInventario = p.Existencia * p.PrecioCosto,
            Estado = p.Existencia == 0 ? "Agotado" : p.Existencia <= 10 ? "Stock bajo" : "Disponible"
        }).ToList();
    }

    [HttpGet("compras")]
    public async Task<ActionResult<List<ReporteCompraDto>>> Compras(DateTime? desde, DateTime? hasta)
    {
        var q = db.Compras.AsNoTracking().Include(c => c.Proveedor).Include(c => c.Detalles).AsQueryable();
        if (desde.HasValue) q = q.Where(c => c.FechaOrden >= desde.Value.Date);
        if (hasta.HasValue) q = q.Where(c => c.FechaOrden < hasta.Value.Date.AddDays(1));

        var lista = await q.OrderByDescending(c => c.FechaOrden).ToListAsync();
        return lista.Select(c => new ReporteCompraDto
        {
            IdCompra = c.IdCompra,
            Fecha = c.FechaOrden,
            NoOrden = c.NoOrdenCompra,
            Proveedor = c.Proveedor?.Nombre ?? "",
            Articulos = c.Detalles.Sum(d => d.Cantidad),
            Total = c.Total
        }).ToList();
    }

    [HttpGet("ventas")]
    public async Task<ActionResult<List<ReporteVentaDto>>> Ventas(DateTime? desde, DateTime? hasta)
    {
        var q = db.Ventas.AsNoTracking().Include(v => v.Cliente).Include(v => v.Empleado).AsQueryable();
        if (desde.HasValue) q = q.Where(v => v.FechaFactura >= desde.Value.Date);
        if (hasta.HasValue) q = q.Where(v => v.FechaFactura < hasta.Value.Date.AddDays(1));

        var lista = await q.OrderByDescending(v => v.FechaFactura).ToListAsync();
        return lista.Select(v => new ReporteVentaDto
        {
            IdVenta = v.IdVenta,
            Fecha = v.FechaFactura,
            Factura = $"{v.Serie}-{v.NoFactura}",
            Cliente = v.Cliente is null ? "" : $"{v.Cliente.Nombres} {v.Cliente.Apellidos}",
            Empleado = v.Empleado is null ? "" : $"{v.Empleado.Nombres} {v.Empleado.Apellidos}",
            Total = v.Total
        }).ToList();
    }
}
