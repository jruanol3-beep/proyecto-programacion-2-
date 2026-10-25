using PuntoVenta.Shared.Dtos;
using PuntoVenta.Shared.Models;

namespace PuntoVenta.Web.Services;

/// <summary>Linea del carrito de una venta en curso.</summary>
public sealed class LineaVenta
{
    public Producto Producto { get; init; } = default!;
    public int Cantidad { get; set; }
}

/// <summary>Venta en curso. Es un servicio "scoped" (vive mientras dure la conexion del usuario),
/// asi el carrito no se pierde al cambiar de pantalla dentro del sistema.</summary>
public class BorradorVenta
{
    public bool Activo { get; set; }
    public VentaCrearDto Nueva { get; set; } = new();
    public List<LineaVenta> Lineas { get; } = new();
    public Cliente? Cliente { get; set; }

    public void Reiniciar()
    {
        Nueva = new VentaCrearDto();
        Lineas.Clear();
        Cliente = null;
    }
}

/// <summary>Compra en curso (misma idea que BorradorVenta).</summary>
public class BorradorCompra
{
    public bool Activo { get; set; }
    public CompraCrearDto Nueva { get; set; } = new();
}
