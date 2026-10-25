using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PuntoVenta.Api.Data;
using PuntoVenta.Api.Models;
using PuntoVenta.Shared.Dtos;

namespace PuntoVenta.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ComprasController(AppDbContext db) : ControllerBase
{
    private IQueryable<Compra> Consulta() => db.Compras.AsNoTracking()
        .Include(c => c.Proveedor)
        .Include(c => c.Detalles).ThenInclude(d => d.Producto);

    private static CompraDto ToDto(Compra c) => new()
    {
        IdCompra = c.IdCompra,
        NoOrdenCompra = c.NoOrdenCompra,
        IdProveedor = c.IdProveedor,
        Proveedor = c.Proveedor?.Nombre ?? "",
        FechaOrden = c.FechaOrden,
        Total = c.Total,
        Detalles = c.Detalles.Select(d => new CompraDetalleDto
        {
            IdProducto = d.IdProducto,
            Producto = d.Producto?.Nombre ?? "",
            Cantidad = d.Cantidad,
            PrecioCosto = d.PrecioCostoUnitario
        }).ToList()
    };

    [HttpGet]
    public async Task<ActionResult<List<CompraDto>>> Get()
    {
        var lista = await Consulta().OrderByDescending(c => c.FechaOrden)
            .ThenByDescending(c => c.IdCompra).ToListAsync();
        return lista.Select(ToDto).ToList();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<CompraDto>> Get(int id)
    {
        var c = await Consulta().FirstOrDefaultAsync(x => x.IdCompra == id);
        return c is null ? NotFound() : ToDto(c);
    }

    [HttpPost]
    public async Task<ActionResult<CompraDto>> Post(CompraCrearDto dto)
    {
        if (!await db.Proveedores.AnyAsync(p => p.IdProveedor == dto.IdProveedor))
            return BadRequest(new ErrorDto("El proveedor seleccionado no existe."));

        var ids = dto.Detalles.Select(d => d.IdProducto).Distinct().ToList();
        var productos = await db.Productos.Where(p => ids.Contains(p.IdProducto))
            .ToDictionaryAsync(p => p.IdProducto);
        if (productos.Count != ids.Count)
            return BadRequest(new ErrorDto("Alguno de los productos seleccionados no existe."));

        await using var tx = await db.Database.BeginTransactionAsync();

        var compra = new Compra
        {
            NoOrdenCompra = (await db.Compras.MaxAsync(c => (int?)c.NoOrdenCompra) ?? 0) + 1,
            IdProveedor = dto.IdProveedor,
            FechaOrden = dto.FechaOrden,
            FechaIngreso = DateTime.Now
        };

        foreach (var d in dto.Detalles)
        {
            compra.Detalles.Add(new CompraDetalle
            {
                IdProducto = d.IdProducto,
                Cantidad = d.Cantidad,
                PrecioCostoUnitario = d.PrecioCosto
            });

            var p = productos[d.IdProducto];
            p.Existencia += d.Cantidad;                      // suma la existencia
            p.PrecioCosto = d.PrecioCosto;                   // actualiza el costo con el de la compra
            p.PrecioVenta = Math.Round(d.PrecioCosto * 1.25m, 2, MidpointRounding.AwayFromZero); // costo + 25 %
        }

        compra.Total = compra.Detalles.Sum(d => d.Cantidad * d.PrecioCostoUnitario);

        db.Compras.Add(compra);
        try
        {
            await db.SaveChangesAsync();
            await tx.CommitAsync();
        }
        catch (DbUpdateException)
        {
            return BadRequest(new ErrorDto("No se pudo registrar la compra. Intente de nuevo."));
        }

        var creada = await Consulta().FirstAsync(c => c.IdCompra == compra.IdCompra);
        return Ok(ToDto(creada));
    }
}

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class VentasController(AppDbContext db) : ControllerBase
{
    private IQueryable<Venta> Consulta() => db.Ventas.AsNoTracking()
        .Include(v => v.Cliente)
        .Include(v => v.Empleado)
        .Include(v => v.Detalles).ThenInclude(d => d.Producto);

    private static VentaDto ToDto(Venta v) => new()
    {
        IdVenta = v.IdVenta,
        Serie = v.Serie,
        NoFactura = v.NoFactura,
        FechaFactura = v.FechaFactura,
        IdCliente = v.IdCliente,
        Cliente = v.Cliente is null ? "" : $"{v.Cliente.Nombres} {v.Cliente.Apellidos}",
        IdEmpleado = v.IdEmpleado,
        Empleado = v.Empleado is null ? "" : $"{v.Empleado.Nombres} {v.Empleado.Apellidos}",
        Total = v.Total,
        Detalles = v.Detalles.Select(d => new VentaDetalleDto
        {
            IdProducto = d.IdProducto,
            Producto = d.Producto?.Nombre ?? "",
            Cantidad = d.Cantidad,
            PrecioUnitario = d.PrecioUnitario
        }).ToList()
    };

    [HttpGet]
    public async Task<ActionResult<List<VentaDto>>> Get()
    {
        var lista = await Consulta().OrderByDescending(v => v.FechaFactura)
            .ThenByDescending(v => v.IdVenta).ToListAsync();
        return lista.Select(ToDto).ToList();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<VentaDto>> Get(int id)
    {
        var v = await Consulta().FirstOrDefaultAsync(x => x.IdVenta == id);
        return v is null ? NotFound() : ToDto(v);
    }

    [HttpPost]
    public async Task<ActionResult<VentaDto>> Post(VentaCrearDto dto)
    {
        if (!await db.Clientes.AnyAsync(c => c.IdCliente == dto.IdCliente))
            return BadRequest(new ErrorDto("El cliente seleccionado no existe."));
        if (!await db.Empleados.AnyAsync(e => e.IdEmpleado == dto.IdEmpleado))
            return BadRequest(new ErrorDto("El empleado seleccionado no existe."));

        var ids = dto.Detalles.Select(d => d.IdProducto).Distinct().ToList();
        var productos = await db.Productos.Where(p => ids.Contains(p.IdProducto))
            .ToDictionaryAsync(p => p.IdProducto);
        if (productos.Count != ids.Count)
            return BadRequest(new ErrorDto("Alguno de los productos seleccionados no existe."));

        // Verifica existencia (sumando las cantidades si el mismo producto se repite)
        foreach (var g in dto.Detalles.GroupBy(d => d.IdProducto))
        {
            var p = productos[g.Key];
            var pedido = g.Sum(x => x.Cantidad);
            if (pedido > p.Existencia)
                return BadRequest(new ErrorDto(
                    $"No se puede realizar la venta. Existencia insuficiente de '{p.Nombre}' " +
                    $"(disponible: {p.Existencia}, solicitado: {pedido})."));
        }

        await using var tx = await db.Database.BeginTransactionAsync();

        var venta = new Venta
        {
            Serie = "A",
            NoFactura = (await db.Ventas.Where(v => v.Serie == "A")
                .MaxAsync(v => (int?)v.NoFactura) ?? 0) + 1,
            FechaFactura = dto.FechaFactura,
            IdCliente = dto.IdCliente,
            IdEmpleado = dto.IdEmpleado,
            FechaIngreso = DateTime.Now
        };

        foreach (var d in dto.Detalles)
        {
            var p = productos[d.IdProducto];
            venta.Detalles.Add(new VentaDetalle
            {
                IdProducto = d.IdProducto,
                Cantidad = d.Cantidad,
                PrecioUnitario = p.PrecioVenta        // siempre el precio_venta del producto
            });
            p.Existencia -= d.Cantidad;               // descuenta la existencia
        }

        venta.Total = venta.Detalles.Sum(d => d.Cantidad * d.PrecioUnitario);

        db.Ventas.Add(venta);
        try
        {
            await db.SaveChangesAsync();
            await tx.CommitAsync();
        }
        catch (DbUpdateException)
        {
            // Otra venta se registro al mismo tiempo (existencia agotada o numero de factura repetido)
            return BadRequest(new ErrorDto("No se pudo registrar la venta. Existencia insuficiente o venta simultanea; revise e intente de nuevo."));
        }

        var creada = await Consulta().FirstAsync(v => v.IdVenta == venta.IdVenta);
        return Ok(ToDto(creada));
    }
}