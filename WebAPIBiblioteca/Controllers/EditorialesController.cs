using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPIBiblioteca.DTOs;
using WebAPIBiblioteca.Models;

namespace WebAPIBiblioteca.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EditorialesController : ControllerBase
    {
        private readonly MiBibliotecaContext context;

        public EditorialesController(MiBibliotecaContext context)
        {
            this.context = context;
        }

        [HttpGet]
        public async Task<ActionResult> GetEditoriales()
        {
            var editoriales = await context.Editoriales.ToListAsync();
            return Ok(editoriales);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetEditorialPorId(int id)
        {
            var editorial = await context.Editoriales.FindAsync(id);
            if (editorial == null)
            {
                return NotFound();
            }
            return Ok(editorial);
        }

        [HttpGet("editorialeslibros")]
        public async Task<ActionResult> GetEditorialesLibros()
        {
            var editoriales = await context.Editoriales.Include(x => x.Libros).ToListAsync();
            
            return Ok(editoriales);
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> PostEditorial(DTOEditorial editorial)
        {
            var newEditorial = new Editoriale()
            {
                Nombre = editorial.Nombre
            };

            await context.AddAsync(newEditorial);
            await context.SaveChangesAsync();

            return Created();
        }

        [HttpPut]
        [Authorize]
        public async Task<ActionResult> PutEditorial(DTOEditorial editorial)
        {
            var editorialUpdate = await context.Editoriales.FindAsync(editorial.Id);
            if (editorialUpdate == null)
            {
                return NotFound();
            }
            editorialUpdate.Nombre = editorial.Nombre;
            await context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        [Authorize]
        public async Task<ActionResult> DeleteEditorial(int id)
        {
            var hayLibros = await context.Libros.AnyAsync(x => x.EditorialId == id);
            if (hayLibros)
            {
                return BadRequest("Hay libros relacionados");
            }

            var editorial = await context.Editoriales.FindAsync(id);

            if (editorial is null)
            {
                return NotFound();
            }

            context.Remove(editorial);
            await context.SaveChangesAsync();
            return Ok();
        }

        [HttpPut("sql")]
        [Authorize]
        public async Task<ActionResult> ModificarEditoral(DTOEditorial editorial)
        {
            await context.Database
                .ExecuteSqlInterpolatedAsync($@"UPDATE Editoriales SET Nombre={editorial.Nombre} WHERE IdEditorial={editorial.Id}");

            return Ok();
        }
    }
}
