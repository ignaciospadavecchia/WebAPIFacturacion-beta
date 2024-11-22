using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPIAlmacen.DTOs;
using WebAPIBiblioteca.DTOs;
using WebAPIBiblioteca.Models;
using WebAPIBiblioteca.Services;

namespace WebAPIBiblioteca.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LibrosController : ControllerBase
    {
        private readonly MiBibliotecaContext context;
        private readonly OperacionesService operacionesService;
        private readonly GestorArchivosService gestorArchivosService;
        public LibrosController(MiBibliotecaContext context, OperacionesService operacionesService, GestorArchivosService gestorArchivosService)
        {
            this.context = context;
            this.operacionesService = operacionesService;
            this.gestorArchivosService = gestorArchivosService;
        }

        [HttpGet("/libros")]
        public async Task<ActionResult> GetLibrosAbsoluta()
        {
            var libros = await context.Libros.ToListAsync();
            return Ok(libros);
        }

        [HttpGet]
        public async Task<ActionResult> GetLibros()
        {
            var tieneAcceso = await operacionesService.PermisoAcceso();
            if (tieneAcceso == false)
            {
                return BadRequest("No han pasado 30 segundos desde tu última petición");
            }
            var libros = await context.Libros.ToListAsync();
            await operacionesService.AddOperacion("Consultar", "Libros");
            return Ok(libros);
        }

        [HttpGet("{isbn}")]
        public async Task<ActionResult> GetLibroPorIsbn(string isbn)
        {
            await operacionesService.AddOperacion("Consulta de libro: " + isbn, "Libros");
            var libro = await context.Libros.FindAsync(isbn);
            if (libro == null)
            {
                return NotFound();
            }

            return Ok(libro);
        }

        [HttpGet("titulo/contiene/{texto}")]
        public async Task<ActionResult> GetLibrosTituloContieneTexto(string texto)
        {
            var libros = await context.Libros.Where(x => x.Titulo.Contains(texto)).ToListAsync();
            return Ok(libros);
        }

        [HttpGet("ordenadosportitulo/{direccion}")]
        public async Task<ActionResult> GetFamiliasOrdenadasPorNombreAscDesc(bool direccion)
        {
            if (direccion)
            {
                var libros =
                    await context.Libros.OrderBy(x => x.Titulo).ToListAsync();
                return Ok(libros);
            }
            else
            {
                var libros =
                    await context.Libros.OrderByDescending(x => x.Titulo).ToListAsync();
                return Ok(libros);
            }
        }

        [HttpGet("entredosprecios")]
        public async Task<ActionResult> GetLibrosEntreDosPreciosQueryString([FromQuery] decimal desde, [FromQuery] decimal hasta)
        {
            var libros =
                await context.Libros.Where(x => x.Precio >= desde && x.Precio <= hasta).ToListAsync();

            return Ok(libros);
        }

        [HttpGet("desdehasta/{desde}/{hasta}")]
        public async Task<ActionResult> GetProductosPaginacion(int desde, int hasta)
        {
            var libros = await context.Libros.Skip(desde).Take(hasta - desde).ToListAsync();
            return Ok(libros);
        }

        [HttpGet("venta")]
        public async Task<ActionResult> GetLibrosVenta()
        {
            var libros = await context.Libros
                .Select(x => new DTOVentaLibro { TituloLibro = x.Titulo, PrecioLibro = x.Precio })
                .ToListAsync();

            return Ok(libros);
        }

        [HttpGet("librosagrupadospordescatalogado")]
        public async Task<ActionResult> GetLibrosAgrupadosPorDescatalogado()
        {
            var libros = await context.Libros.GroupBy(g => g.Descatalogados)
                .Select(x => new
                {
                    Descatalogado = x.Key,
                    Total = x.Count(),
                    Libros = x.ToList()
                }).ToListAsync();

            return Ok(libros);
        }

        // Filtrado múltiple como debe hacerse
        [HttpGet("filtromultiple/asqueryable")]
        public async Task<ActionResult> GetLibrosAsQueryable([FromQuery] DTOLibrosFiltro filtro)
        {
            var consulta = context.Libros.AsQueryable();

            if (filtro.Precio > 0)
            {
                consulta = consulta.Where(x => x.Precio > filtro.Precio);
            }

            consulta = consulta.Where(x => x.Descatalogados == filtro.Descatalogado);

            var libros = await consulta.ToListAsync();

            return Ok(libros);
        }

        [HttpPost]
        [Authorize]
        public async Task<ActionResult> PostLibro(DTOLibro libro)
        {
            var existeAutor = await context.Autores.AnyAsync(x => x.IdAutor == libro.AutorId);
            if (!existeAutor)
            {
                return BadRequest("Autor inexistente");
            }

            var existeEditorial = await context.Editoriales.AnyAsync(x => x.IdEditorial == libro.EditorialId);
            if (!existeEditorial)
            {
                return BadRequest("Editorial inexistente");
            }

            var newLibro = new Libro()
            {
                Isbn = libro.Isbn,
                Titulo = libro.Titulo,
                Precio = libro.Precio,
                Paginas = libro.Paginas,
                Descatalogados = libro.Descatalogados,
                AutorId = libro.AutorId,
                EditorialId = libro.EditorialId,
            };

            await context.AddAsync(newLibro);
            await context.SaveChangesAsync();

            return Created();
        }

        [HttpPost("imagen")]
        [Authorize]
        public async Task<ActionResult> PostProductos([FromForm] DTOAgregarLibroImagen libro)
        {
            Libro newLibro = new Libro
            {
                Isbn=libro.Isbn,
                Titulo = libro.Titulo,
                Precio = libro.Precio,
                Descatalogados = false,
                AutorId = libro.AutorId,
                EditorialId = libro.EditorialId,
                Paginas = libro.Paginas,
                FotoPortadaUrl = ""
            };

            if (libro.Foto != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    // Extraemos la imagen de la petición
                    await libro.Foto.CopyToAsync(memoryStream);
                    // La convertimos a un array de bytes que es lo que necesita el método de guardar
                    var contenido = memoryStream.ToArray();
                    // Recibimos el nombre del archivo
                    // El servicio Transient GestorArchivosLocal instancia el servicio y cuando se deja de usar se destruye
                    newLibro.FotoPortadaUrl = await gestorArchivosService.GuardarArchivo(contenido, libro.Foto.FileName, "imagenes");
                }
            }

            await context.AddAsync(newLibro);
            await context.SaveChangesAsync();
            return Ok(newLibro);
        }

        [HttpPost("varios")]
        [Authorize]
        public async Task<ActionResult> PostLibros(List<DTOLibro> libros)
        {
            foreach (var libro in libros)
            {
                var existeAutor = await context.Autores.AnyAsync(x => x.IdAutor == libro.AutorId);
                if (!existeAutor)
                {
                    return BadRequest("Autor inexistente");
                }

                var existeEditorial = await context.Editoriales.AnyAsync(x => x.IdEditorial == libro.EditorialId);
                if (!existeEditorial)
                {
                    return BadRequest("Editorial inexistente");
                }

                var newLibro = new Libro()
                {
                    Isbn = libro.Isbn,
                    Titulo = libro.Titulo,
                    Precio = libro.Precio,
                    Paginas = libro.Paginas,
                    Descatalogados = libro.Descatalogados,
                    AutorId = libro.AutorId,
                    EditorialId = libro.EditorialId,
                };

                await context.AddAsync(newLibro);
                await context.SaveChangesAsync();

            }

            return Created();
        }

        [HttpPut("imagen")]
        [Authorize]
        public async Task<ActionResult> PutProductos([FromForm] DTOAgregarLibroImagen libro)
        {
            var libroActualizar = await context.Libros.FindAsync(libro.Isbn);
            if (libroActualizar == null)
            {
                return NotFound();
            }

            libroActualizar.Titulo = libro.Titulo;
            libroActualizar.Precio = libro.Precio;
            libroActualizar.Paginas = libro.Paginas;
            libroActualizar.AutorId = libro.AutorId;
            libroActualizar.EditorialId = libro.EditorialId;

            if (libro.Foto != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await libro.Foto.CopyToAsync(memoryStream);
                    var contenido = memoryStream.ToArray();
                    libroActualizar.FotoPortadaUrl = await gestorArchivosService.EditarArchivo(contenido, libro.Foto.FileName, "imagenes", libroActualizar.FotoPortadaUrl);
                }
            }

            await context.SaveChangesAsync();
            return Ok(libroActualizar);
        }

        [HttpDelete("{id:int}")]
        [Authorize]
        public async Task<ActionResult> DeleteLibro(int id)
        {
            var libro = await context.Libros.FindAsync(id);

            if (libro is null)
            {
                return NotFound();
            }

            context.Remove(libro);
            await context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("imagen/{id}")]
        [Authorize]
        public async Task<ActionResult> DeleteLibrosImagen(int id)
        {
            var libro = await context.Libros.FindAsync(id);
            if (libro == null)
            {
                return NotFound();
            }

            await gestorArchivosService.BorrarArchivo(libro.FotoPortadaUrl, "imagenes");
            context.Remove(libro);
            await context.SaveChangesAsync();
            return Ok();
        }
    }
}
