using System.Globalization;
using System.Net;
using System.Text;
using PuntoVenta.Shared.Dtos;

namespace PuntoVenta.Web.Services;

/// <summary>Arma el HTML imprimible de una factura de venta y de una orden de compra.
/// Se imprime desde un iframe oculto (ver wwwroot/pv.js), sin tocar la pagina actual.</summary>
public static class DocumentoImpresion
{
    private static readonly CultureInfo Cultura = CultureInfo.GetCultureInfo("es-GT");

    private static string H(string? s) => WebUtility.HtmlEncode(s ?? "");
    private static string Q(decimal v) => "Q " + v.ToString("N2", CultureInfo.InvariantCulture);

    public static string Factura(VentaDto v)
    {
        var filas = new StringBuilder();
        foreach (var d in v.Detalles)
            filas.Append($"<tr><td>{H(d.Producto)}</td><td class='r'>{d.Cantidad}</td><td class='r'>{Q(d.PrecioUnitario)}</td><td class='r'>{Q(d.Subtotal)}</td></tr>");

        var cuerpo = $@"
<h1>Punto de Venta</h1>
<h2>FACTURA {H(v.Serie)}-{v.NoFactura}</h2>
<p><b>Fecha:</b> {v.FechaFactura.ToString("dd/MM/yyyy", Cultura)}<br/>
<b>Cliente:</b> {H(v.Cliente)}<br/>
<b>Atendio:</b> {H(v.Empleado)}</p>
<table>
<thead><tr><th>Producto</th><th class='r'>Cant.</th><th class='r'>Precio</th><th class='r'>Subtotal</th></tr></thead>
<tbody>{filas}</tbody>
<tfoot><tr><th colspan='3' class='r'>TOTAL</th><th class='r'>{Q(v.Total)}</th></tr></tfoot>
</table>
<p class='pie'>Gracias por su compra</p>";
        return Pagina($"Factura {v.Serie}-{v.NoFactura}", cuerpo);
    }

    public static string OrdenCompra(CompraDto c)
    {
        var filas = new StringBuilder();
        foreach (var d in c.Detalles)
            filas.Append($"<tr><td>{H(d.Producto)}</td><td class='r'>{d.Cantidad}</td><td class='r'>{Q(d.PrecioCosto)}</td><td class='r'>{Q(d.Subtotal)}</td></tr>");

        var cuerpo = $@"
<h1>Punto de Venta</h1>
<h2>ORDEN DE COMPRA No. {c.NoOrdenCompra}</h2>
<p><b>Fecha:</b> {c.FechaOrden.ToString("dd/MM/yyyy", Cultura)}<br/>
<b>Proveedor:</b> {H(c.Proveedor)}</p>
<table>
<thead><tr><th>Producto</th><th class='r'>Cant.</th><th class='r'>Precio costo</th><th class='r'>Subtotal</th></tr></thead>
<tbody>{filas}</tbody>
<tfoot><tr><th colspan='3' class='r'>TOTAL</th><th class='r'>{Q(c.Total)}</th></tr></tfoot>
</table>";
        return Pagina($"Orden de compra {c.NoOrdenCompra}", cuerpo);
    }

    private static string Pagina(string titulo, string cuerpo) => $@"<!DOCTYPE html>
<html lang='es'><head><meta charset='utf-8'/><title>{H(titulo)}</title>
<style>
 body {{ font-family: Arial, Helvetica, sans-serif; font-size: 13px; margin: 24px; color: #111; }}
 h1 {{ text-align: center; margin: 0; font-size: 22px; }}
 h2 {{ text-align: center; margin: 4px 0 16px; font-size: 16px; }}
 table {{ width: 100%; border-collapse: collapse; margin-top: 8px; }}
 th, td {{ border-bottom: 1px solid #bbb; padding: 5px 6px; text-align: left; }}
 thead th {{ background: #eee; }}
 .r {{ text-align: right; }}
 tfoot th {{ font-size: 15px; border-top: 2px solid #111; border-bottom: none; }}
 .pie {{ text-align: center; margin-top: 24px; color: #555; }}
</style></head><body>{cuerpo}</body></html>";
}
