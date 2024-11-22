using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Runtime.InteropServices;
using WebAPIBiblioteca.DTOs;
using WebAPIBiblioteca.Models;

namespace WebAPIBiblioteca.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AutoresController : ControllerBase
    {
        private readonly MiBibliotecaContext context;

        public AutoresController(MiBibliotecaContext context)
        {
            this.context = context;
        }

        [HttpGet]
        public async Task<ActionResult> GetAutores()
        {
            var autores = await context.Autores.ToListAsync();
            return Ok(autores);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetAutorPorId(int id)
        {
            var autor = await context.Autores.FindAsync(id);
            if (autor == null) {
                return NotFound();
            }
            return Ok(autor);
        }

        [HttpGet("todosautoreslibros")]
        public async Task<ActionResult> GetTodosAutoresLibros()
        {
            var autores = await context.Autores
                       .Select(x => new DTOAutoresLibros
                       {
                           IdAutor = x.IdAutor,
                           Nombre = x.Nombre,
                           TotalLibros = x.Libros.Count(),
                           PrecioPromedio = x.Libros.Average(x => x.Precio),
                           Libros = x.Libros.Select(y => new DTOLibroItem
                           {
                               Isbn = y.Isbn,
                               Titulo = y.Titulo,
                               Precio = y.Precio,
                           }).ToList(),
                       }).ToListAsync();

            return Ok(autores);
        }

        [HttpGet("autoreslibros/{id:int}")]
        public async Task<ActionResult> GetAutoresLibros(int id)
        {
            var autor = await context.Autores
                       .Select(x => new DTOAutoresLibros
                       {
                           IdAutor = x.IdAutor,
                           Nombre = x.Nombre,
                           TotalLibros = x.Libros.Count(),
                           PrecioPromedio = x.Libros.Average(x=>x.Precio),
                           Libros = x.Libros.Select(y => new DTOLibroItem
                           {
                               Isbn = y.Isbn,
                               Titulo = y.Titulo,
                               Precio = y.Precio,
                           }).ToList(),
                       }).FirstOrDefaultAsync(x => x.IdAutor == id);
            if (autor == null)
            {
                return NotFound();
            }
            return Ok(autor);
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> PostAutor(DTOAutor autor)
        {

            var newAutor = new Autore()
            {
                Nombre = autor.Nombre
            };

            await context.AddAsync(newAutor);
            await context.SaveChangesAsync();

            return Created();
        }

        [HttpPut]
        [Authorize]
        public async Task<ActionResult> PutAutor(DTOAutor autor)
        {
            var autorUpdate = await context.Autores.AsTracking().FirstOrDefaultAsync(x => x.IdAutor == autor.Id);
            if (autorUpdate == null)
            {
                return NotFound();
            }
            autorUpdate.Nombre = autor.Nombre;
            await context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        [Authorize]
        public async Task<ActionResult> DeleteAutor(int id)
        {
            var hayLibros = await context.Libros.AnyAsync(x => x.AutorId == id);
            if (hayLibros)
            {
                return BadRequest("Hay libros relacionados");
            }
            var autor = await context.Autores.FirstOrDefaultAsync(x => x.IdAutor == id);

            if (autor is null)
            {
                return NotFound();
            }

            context.Remove(autor);
            await context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("sql/{id:int}")]
        [Authorize]
        public async Task<ActionResult> EliminarAutorPorSQL(int id)
        {
            var libro = await context.Libros
                        .FromSqlInterpolated($"SELECT * FROM Libros WHERE AutorId = {id}")
                        .FirstOrDefaultAsync();

            if (libro != null)
            {
                return BadRequest("No se puede eliminar el autor porque tiene libros asociados");
            }

            await context.Database
                .ExecuteSqlInterpolatedAsync($@"DELETE FROM Autores WHERE IdAutor={id}");

            return Ok();
        }
    }
}
