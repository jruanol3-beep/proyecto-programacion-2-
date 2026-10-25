using Microsoft.EntityFrameworkCore;
using PuntoVenta.Api.Models;
using PuntoVenta.Shared.Models;

namespace PuntoVenta.Api.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Puesto> Puestos => Set<Puesto>();
    public DbSet<Empleado> Empleados => Set<Empleado>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Proveedor> Proveedores => Set<Proveedor>();
    public DbSet<Marca> Marcas => Set<Marca>();
    public DbSet<Producto> Productos => Set<Producto>();
    public DbSet<Compra> Compras => Set<Compra>();
    public DbSet<CompraDetalle> ComprasDetalle => Set<CompraDetalle>();
    public DbSet<Venta> Ventas => Set<Venta>();
    public DbSet<VentaDetalle> VentasDetalle => Set<VentaDetalle>();
    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<MenuItem> Menu => Set<MenuItem>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        mb.Entity<Compra>()
            .HasMany(c => c.Detalles).WithOne(d => d.Compra)
            .HasForeignKey(d => d.IdCompra).OnDelete(DeleteBehavior.Cascade);

        mb.Entity<Venta>()
            .HasMany(v => v.Detalles).WithOne(d => d.Venta)
            .HasForeignKey(d => d.IdVenta).OnDelete(DeleteBehavior.Cascade);
    }
}
