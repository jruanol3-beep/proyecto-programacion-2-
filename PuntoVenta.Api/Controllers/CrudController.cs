using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PuntoVenta.Api.Data;
using PuntoVenta.Shared.Dtos;

namespace PuntoVenta.Api.Controllers;

[ApiController]
[Authorize]
public abstract class CrudController<T>(AppDbContext db) : ControllerBase where T : class
{
    protected readonly AppDbContext Db = db;

    private string KeyName =>
        Db.Model.FindEntityType(typeof(T))!.FindPrimaryKey()!.Properties[0].Name;

    protected int GetId(T e) => (int)Db.Entry(e).Property(KeyName).CurrentValue!;

    protected virtual IQueryable<T> Consulta() => Db.Set<T>().AsNoTracking();
    protected virtual IQueryable<T> Filtrar(IQueryable<T> q, string buscar) => q;
    protected virtual void Preparar(T e) { }
    protected virtual Task<string?> ValidarAsync(T e) => Task.FromResult<string?>(null);

    [HttpGet]
    public virtual async Task<ActionResult<List<T>>> Get([FromQuery] string? buscar = null)
    {
        var q = Consulta();
        if (!string.IsNullOrWhiteSpace(buscar)) q = Filtrar(q, buscar.Trim());
        return await q.ToListAsync();
    }

    [HttpGet("{id:int}")]
    public virtual async Task<ActionResult<T>> Get(int id)
    {
        var key = KeyName;
        var e = await Consulta().FirstOrDefaultAsync(x => EF.Property<int>(x, key) == id);
        return e is null ? NotFound() : e;
    }

    [HttpPost]
    public virtual async Task<ActionResult<T>> Post(T entity)
    {
        Preparar(entity);
        var error = await ValidarAsync(entity);
        if (error is not null) return BadRequest(new ErrorDto(error));

        Db.Set<T>().Add(entity);
        await Db.SaveChangesAsync();
        return Ok(entity);
    }

    [HttpPut("{id:int}")]
    public virtual async Task<IActionResult> Put(int id, T entity)
    {
        if (GetId(entity) != id) return BadRequest(new ErrorDto("El id no coincide."));

        Preparar(entity);
        var error = await ValidarAsync(entity);
        if (error is not null) return BadRequest(new ErrorDto(error));

        var actual = await Db.Set<T>().FindAsync(id);
        if (actual is null) return NotFound();

        Db.Entry(actual).CurrentValues.SetValues(entity);
        await Db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public virtual async Task<IActionResult> Delete(int id)
    {
        var actual = await Db.Set<T>().FindAsync(id);
        if (actual is null) return NotFound();

        try
        {
            Db.Set<T>().Remove(actual);
            await Db.SaveChangesAsync();
        }
        catch (DbUpdateException)
        {
            return Conflict(new ErrorDto("No se puede eliminar: el registro esta en uso."));
        }
        return NoContent();
    }
}