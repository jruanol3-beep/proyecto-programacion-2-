using System.ComponentModel.DataAnnotations;

namespace PuntoVenta.Shared.Dtos;

public record ErrorDto(string Mensaje);

// ---------------- Autenticacion ----------------
public class LoginRequest
{
    [Required(ErrorMessage = "El usuario es obligatorio")]
    [StringLength(40, ErrorMessage = "Maximo 40 caracteres")]
    public string Usuario { get; set; } = "";

    [Required(ErrorMessage = "La contrasena es obligatoria")]
    [StringLength(100, ErrorMessage = "Maximo 100 caracteres")]
    public string Password { get; set; } = "";
}

public class LoginResponse
{
    public string Token { get; set; } = "";
    public string Usuario { get; set; } = "";
    public string NombreCompleto { get; set; } = "";
    public string Rol { get; set; } = "";
    public DateTime Expira { get; set; }
}

// ---------------- Menu dinamico ----------------
public class MenuDto
{
    public int IdMenu { get; set; }
    public string Nombre { get; set; } = "";
    public string? Ruta { get; set; }
    public string? Icono { get; set; }
    public List<MenuDto> Hijos { get; set; } = new();
}

// ---------------- Compras ----------------
public class CompraDetalleCrearDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un producto")]
    public int IdProducto { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a cero")]
    public int Cantidad { get; set; }

    [Range(0.01, 99999999.99, ErrorMessage = "El precio debe ser mayor a cero")]
    public decimal PrecioCosto { get; set; }
}

public class CompraCrearDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un proveedor")]
    public int IdProveedor { get; set; }

    public DateTime FechaOrden { get; set; } = DateTime.Today;

    [MinLength(1, ErrorMessage = "La compra debe tener al menos un producto")]
    public List<CompraDetalleCrearDto> Detalles { get; set; } = new();
}

public class CompraDetalleDto
{
    public int IdProducto { get; set; }
    public string Producto { get; set; } = "";
    public int Cantidad { get; set; }
    public decimal PrecioCosto { get; set; }
    public decimal Subtotal => Cantidad * PrecioCosto;
}

public class CompraDto
{
    public int IdCompra { get; set; }
    public int NoOrdenCompra { get; set; }
    public int IdProveedor { get; set; }
    public string Proveedor { get; set; } = "";
    public DateTime FechaOrden { get; set; }
    public decimal Total { get; set; }
    public List<CompraDetalleDto> Detalles { get; set; } = new();
}

// ---------------- Ventas ----------------
public class VentaDetalleCrearDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un producto")]
    public int IdProducto { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor a cero")]
    public int Cantidad { get; set; }
}

public class VentaCrearDto
{
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un cliente")]
    public int IdCliente { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un empleado")]
    public int IdEmpleado { get; set; }

    public DateTime FechaFactura { get; set; } = DateTime.Today;

    [MinLength(1, ErrorMessage = "La venta debe tener al menos un producto")]
    public List<VentaDetalleCrearDto> Detalles { get; set; } = new();
}

public class VentaDetalleDto
{
    public int IdProducto { get; set; }
    public string Producto { get; set; } = "";
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal => Cantidad * PrecioUnitario;
}

public class VentaDto
{
    public int IdVenta { get; set; }
    public string Serie { get; set; } = "";
    public int NoFactura { get; set; }
    public DateTime FechaFactura { get; set; }
    public int IdCliente { get; set; }
    public string Cliente { get; set; } = "";
    public int IdEmpleado { get; set; }
    public string Empleado { get; set; } = "";
    public decimal Total { get; set; }
    public List<VentaDetalleDto> Detalles { get; set; } = new();
}

// ---------------- Reportes ----------------
public class ReporteProductoDto
{
    public string Producto { get; set; } = "";
    public string Marca { get; set; } = "";
    public int Existencia { get; set; }
    public decimal PrecioCosto { get; set; }
    public decimal PrecioVenta { get; set; }
}

public class ReporteExistenciaDto
{
    public string Producto { get; set; } = "";
    public string Marca { get; set; } = "";
    public int Existencia { get; set; }
    public decimal ValorInventario { get; set; }
    public string Estado { get; set; } = "";
}

public class ReporteCompraDto
{
    public int IdCompra { get; set; }
    public DateTime Fecha { get; set; }
    public int NoOrden { get; set; }
    public string Proveedor { get; set; } = "";
    public int Articulos { get; set; }
    public decimal Total { get; set; }
}

public class ReporteVentaDto
{
    public int IdVenta { get; set; }
    public DateTime Fecha { get; set; }
    public string Factura { get; set; } = "";
    public string Cliente { get; set; } = "";
    public string Empleado { get; set; } = "";
    public decimal Total { get; set; }
}
