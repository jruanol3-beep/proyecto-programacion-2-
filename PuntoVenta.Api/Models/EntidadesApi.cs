using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PuntoVenta.Shared.Models;

namespace PuntoVenta.Api.Models;

// Entidades que solo usa la API (no se exponen directamente: se usan DTOs).

[Table("roles")]
public class Rol
{
    [Key, Column("id_rol")] public int IdRol { get; set; }
    [Column("rol")] public string Nombre { get; set; } = "";
}

[Table("usuarios")]
public class Usuario
{
    [Key, Column("id_usuario")] public int IdUsuario { get; set; }
    [Column("usuario")] public string NombreUsuario { get; set; } = "";
    [Column("password_hash")] public string PasswordHash { get; set; } = "";
    [Column("nombre_completo")] public string NombreCompleto { get; set; } = "";
    [Column("id_rol")] public int IdRol { get; set; }
    [Column("activo")] public bool Activo { get; set; } = true;
    [Column("fecha_ingreso")] public DateTime FechaIngreso { get; set; } = DateTime.Now;

    [ForeignKey(nameof(IdRol))] public Rol? Rol { get; set; }
}

[Table("menu")]
public class MenuItem
{
    [Key, Column("id_menu")] public int IdMenu { get; set; }
    [Column("nombre")] public string Nombre { get; set; } = "";
    [Column("ruta")] public string? Ruta { get; set; }
    [Column("icono")] public string? Icono { get; set; }
    [Column("id_padre")] public int? IdPadre { get; set; }
    [Column("orden")] public int Orden { get; set; }
    [Column("activo")] public bool Activo { get; set; } = true;
}

[Table("compras")]
public class Compra
{
    [Key, Column("id_compra")] public int IdCompra { get; set; }
    [Column("no_orden_compra")] public int NoOrdenCompra { get; set; }
    [Column("id_proveedor")] public int IdProveedor { get; set; }
    [Column("fecha_orden")] public DateTime FechaOrden { get; set; }
    [Column("total", TypeName = "decimal(12,2)")] public decimal Total { get; set; }
    [Column("fecha_ingreso")] public DateTime FechaIngreso { get; set; } = DateTime.Now;

    [ForeignKey(nameof(IdProveedor))] public Proveedor? Proveedor { get; set; }
    public List<CompraDetalle> Detalles { get; set; } = new();
}

[Table("compras_detalle")]
public class CompraDetalle
{
    [Key, Column("id_compra_detalle")] public int IdCompraDetalle { get; set; }
    [Column("id_compra")] public int IdCompra { get; set; }
    [Column("id_producto")] public int IdProducto { get; set; }
    [Column("cantidad")] public int Cantidad { get; set; }
    [Column("precio_costo_unitario", TypeName = "decimal(10,2)")] public decimal PrecioCostoUnitario { get; set; }

    [ForeignKey(nameof(IdCompra))] public Compra? Compra { get; set; }
    [ForeignKey(nameof(IdProducto))] public Producto? Producto { get; set; }
}

[Table("ventas")]
public class Venta
{
    [Key, Column("id_venta")] public int IdVenta { get; set; }
    [Column("no_factura")] public int NoFactura { get; set; }
    [Column("serie")] public string Serie { get; set; } = "A";
    [Column("fecha_factura")] public DateTime FechaFactura { get; set; }
    [Column("id_cliente")] public int IdCliente { get; set; }
    [Column("id_empleado")] public int IdEmpleado { get; set; }
    [Column("total", TypeName = "decimal(12,2)")] public decimal Total { get; set; }
    [Column("fecha_ingreso")] public DateTime FechaIngreso { get; set; } = DateTime.Now;

    [ForeignKey(nameof(IdCliente))] public Cliente? Cliente { get; set; }
    [ForeignKey(nameof(IdEmpleado))] public Empleado? Empleado { get; set; }
    public List<VentaDetalle> Detalles { get; set; } = new();
}

[Table("ventas_detalle")]
public class VentaDetalle
{
    [Key, Column("id_venta_detalle")] public int IdVentaDetalle { get; set; }
    [Column("id_venta")] public int IdVenta { get; set; }
    [Column("id_producto")] public int IdProducto { get; set; }
    [Column("cantidad")] public int Cantidad { get; set; }
    [Column("precio_unitario", TypeName = "decimal(10,2)")] public decimal PrecioUnitario { get; set; }

    [ForeignKey(nameof(IdVenta))] public Venta? Venta { get; set; }
    [ForeignKey(nameof(IdProducto))] public Producto? Producto { get; set; }
}
