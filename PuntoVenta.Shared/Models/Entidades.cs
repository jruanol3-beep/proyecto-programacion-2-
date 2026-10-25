using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PuntoVenta.Shared.Models;

// Modelos (la "M" de MVC) compartidos entre la API y Blazor.
// Las mismas anotaciones de validacion se usan en ambos lados.

[Table("puestos")]
public class Puesto
{
    [Key, Column("id_puesto")]
    public int IdPuesto { get; set; }

    [Column("puesto")]
    [Required(ErrorMessage = "El nombre del puesto es obligatorio")]
    [StringLength(60, ErrorMessage = "Maximo 60 caracteres")]
    public string Nombre { get; set; } = "";

    [Column("descripcion")]
    [StringLength(150, ErrorMessage = "Maximo 150 caracteres")]
    public string? Descripcion { get; set; }
}

[Table("marcas")]
public class Marca
{
    [Key, Column("id_marca")]
    public int IdMarca { get; set; }

    [Column("marca")]
    [Required(ErrorMessage = "El nombre de la marca es obligatorio")]
    [StringLength(60, ErrorMessage = "Maximo 60 caracteres")]
    public string Nombre { get; set; } = "";
}

[Table("empleados")]
public class Empleado
{
    [Key, Column("id_empleado")]
    public int IdEmpleado { get; set; }

    [Column("nombres")]
    [Required(ErrorMessage = "Los nombres son obligatorios")]
    [StringLength(60, ErrorMessage = "Maximo 60 caracteres")]
    public string Nombres { get; set; } = "";

    [Column("apellidos")]
    [Required(ErrorMessage = "Los apellidos son obligatorios")]
    [StringLength(60, ErrorMessage = "Maximo 60 caracteres")]
    public string Apellidos { get; set; } = "";

    [Column("direccion")]
    [StringLength(150, ErrorMessage = "Maximo 150 caracteres")]
    public string? Direccion { get; set; }

    [Column("telefono")]
    [StringLength(20, ErrorMessage = "Maximo 20 caracteres")]
    [RegularExpression(@"^[0-9\-\+\s]*$", ErrorMessage = "El telefono solo admite numeros")]
    public string? Telefono { get; set; }

    [Column("dpi")]
    [Required(ErrorMessage = "El DPI es obligatorio")]
    [StringLength(20, MinimumLength = 13, ErrorMessage = "El DPI debe tener entre 13 y 20 caracteres")]
    [RegularExpression(@"^[0-9]+$", ErrorMessage = "El DPI solo admite numeros")]
    public string Dpi { get; set; } = "";

    [Column("correo")]
    [StringLength(100, ErrorMessage = "Maximo 100 caracteres")]
    [EmailAddress(ErrorMessage = "Correo electronico no valido")]
    public string? Correo { get; set; }

    [Column("fecha_nacimiento")]
    public DateTime? FechaNacimiento { get; set; }

    [Column("id_puesto")]
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione un puesto")]
    public int IdPuesto { get; set; }

    [Column("fecha_inicio_labores")]
    public DateTime? FechaInicioLabores { get; set; }

    [Column("fecha_ingreso")]
    public DateTime FechaIngreso { get; set; } = DateTime.Now;

    [ForeignKey(nameof(IdPuesto))]
    public Puesto? Puesto { get; set; }
}

[Table("clientes")]
public class Cliente
{
    [Key, Column("id_cliente")]
    public int IdCliente { get; set; }

    [Column("nombres")]
    [Required(ErrorMessage = "Los nombres son obligatorios")]
    [StringLength(60, ErrorMessage = "Maximo 60 caracteres")]
    public string Nombres { get; set; } = "";

    [Column("apellidos")]
    [Required(ErrorMessage = "Los apellidos son obligatorios")]
    [StringLength(60, ErrorMessage = "Maximo 60 caracteres")]
    public string Apellidos { get; set; } = "";

    [Column("nit")]
    [StringLength(15, ErrorMessage = "Maximo 15 caracteres")]
    public string? Nit { get; set; }

    [Column("genero")]
    public bool? Genero { get; set; }

    [Column("telefono")]
    [StringLength(20, ErrorMessage = "Maximo 20 caracteres")]
    [RegularExpression(@"^[0-9\-\+\s]*$", ErrorMessage = "El telefono solo admite numeros")]
    public string? Telefono { get; set; }

    [Column("correo")]
    [StringLength(100, ErrorMessage = "Maximo 100 caracteres")]
    [EmailAddress(ErrorMessage = "Correo electronico no valido")]
    public string? Correo { get; set; }

    [Column("direccion")]
    [StringLength(150, ErrorMessage = "Maximo 150 caracteres")]
    public string? Direccion { get; set; }

    [Column("fecha_ingreso")]
    public DateTime FechaIngreso { get; set; } = DateTime.Now;
}

[Table("proveedores")]
public class Proveedor
{
    [Key, Column("id_proveedor")]
    public int IdProveedor { get; set; }

    [Column("proveedor")]
    [Required(ErrorMessage = "El nombre del proveedor es obligatorio")]
    [StringLength(80, ErrorMessage = "Maximo 80 caracteres")]
    public string Nombre { get; set; } = "";

    [Column("nit")]
    [Required(ErrorMessage = "El NIT es obligatorio")]
    [StringLength(15, ErrorMessage = "Maximo 15 caracteres")]
    public string Nit { get; set; } = "";

    [Column("direccion")]
    [StringLength(150, ErrorMessage = "Maximo 150 caracteres")]
    public string? Direccion { get; set; }

    [Column("telefono")]
    [StringLength(20, ErrorMessage = "Maximo 20 caracteres")]
    [RegularExpression(@"^[0-9\-\+\s]*$", ErrorMessage = "El telefono solo admite numeros")]
    public string? Telefono { get; set; }

    [Column("correo")]
    [StringLength(100, ErrorMessage = "Maximo 100 caracteres")]
    [EmailAddress(ErrorMessage = "Correo electronico no valido")]
    public string? Correo { get; set; }
}

[Table("productos")]
public class Producto
{
    [Key, Column("id_producto")]
    public int IdProducto { get; set; }

    [Column("producto")]
    [Required(ErrorMessage = "El nombre del producto es obligatorio")]
    [StringLength(80, ErrorMessage = "Maximo 80 caracteres")]
    public string Nombre { get; set; } = "";

    [Column("id_marca")]
    [Range(1, int.MaxValue, ErrorMessage = "Seleccione una marca")]
    public int IdMarca { get; set; }

    [Column("descripcion")]
    [StringLength(255, ErrorMessage = "Maximo 255 caracteres")]
    public string? Descripcion { get; set; }

    /// <summary>Solo la ruta de la imagen, ej: /images/productos/monitor.jpg</summary>
    [Column("imagen")]
    [StringLength(255)]
    public string? Imagen { get; set; }

    [Column("precio_costo", TypeName = "decimal(10,2)")]
    [Range(0.01, 99999999.99, ErrorMessage = "El precio de costo debe ser mayor a cero")]
    public decimal PrecioCosto { get; set; }

    [Column("precio_venta", TypeName = "decimal(10,2)")]
    [Range(0.01, 99999999.99, ErrorMessage = "El precio de venta debe ser mayor a cero")]
    public decimal PrecioVenta { get; set; }

    [Column("existencia")]
    [Range(0, int.MaxValue, ErrorMessage = "La existencia no puede ser negativa")]
    public int Existencia { get; set; }

    [Column("fecha_ingreso")]
    public DateTime FechaIngreso { get; set; } = DateTime.Now;

    [ForeignKey(nameof(IdMarca))]
    public Marca? Marca { get; set; }
}
