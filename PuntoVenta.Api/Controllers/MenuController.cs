using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PuntoVenta.Api.Data;
using PuntoVenta.Shared.Dtos;

namespace PuntoVenta.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class MenuController(AppDbContext db) : ControllerBase
{
    /// <summary>Devuelve el menu como arbol (padre-hijo) leido de la tabla menu.</summary>
    [HttpGet]
    public async Task<ActionResult<List<MenuDto>>> Get()
    {
        var items = await db.Menu.AsNoTracking()
            .Where(m => m.Activo)
            .OrderBy(m => m.Orden)
            .ToListAsync();

        var nodos = items.ToDictionary(m => m.IdMenu, m => new MenuDto
        {
            IdMenu = m.IdMenu,
            Nombre = m.Nombre,
            Ruta = m.Ruta,
            Icono = m.Icono
        });

        var raiz = new List<MenuDto>();
        foreach (var m in items)
        {
            if (m.IdPadre is int padre && nodos.TryGetValue(padre, out var nodoPadre))
                nodoPadre.Hijos.Add(nodos[m.IdMenu]);
            else if (m.IdPadre is null)
                raiz.Add(nodos[m.IdMenu]);
        }
        return raiz;
    }
}
