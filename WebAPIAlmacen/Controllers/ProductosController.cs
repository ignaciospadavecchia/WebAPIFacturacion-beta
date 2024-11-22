using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPIAlmacen.DTOs;
using WebAPIAlmacen.Models;
using WebAPIAlmacen.Services;

namespace WebAPIAlmacen.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProductosController : ControllerBase
    {
        private readonly MiAlmacenContext context;
        private readonly GestorArchivosService gestorArchivosService;
        private readonly ContadorPeticionesService contadorPeticionesService;


        public ProductosController(MiAlmacenContext context, GestorArchivosService gestorArchivosService,
             ContadorPeticionesService contadorPeticionesService)
        {
            this.context = context;
            this.gestorArchivosService = gestorArchivosService;
            this.contadorPeticionesService = contadorPeticionesService;
        }

        [HttpGet("entredosprecios/{desde}/{hasta}")]
        [AllowAnonymous]
        public async Task<ActionResult> GetProductosEntreDosPrecios([FromRoute] decimal desde, [FromRoute] decimal hasta)
        {
            contadorPeticionesService.Incrementar();
            var productos =
                await context.Productos.Where(x => x.Precio >= desde && x.Precio <= hasta).ToListAsync();

            return Ok(productos);
        }

        [HttpGet("entredosprecios")]
        public async Task<ActionResult> GetProductosEntreDosPreciosQueryString([FromQuery] decimal desde, [FromQuery] decimal hasta)
        {
            contadorPeticionesService.Incrementar();
            var productos =
                await context.Productos.Where(x => x.Precio >= desde && x.Precio <= hasta).ToListAsync();

            return Ok(productos);
        }

        [HttpGet("paginacion")]
        public async Task<ActionResult> GetProductosPaginacion()
        {
            var productos = await context.Productos.Take(2).ToListAsync();
            var productos2 = await context.Productos.Skip(1).Take(2).ToListAsync();
            return Ok(productos2);
        }

        [HttpGet("paginacion/{pagina?}")]
        public async Task<ActionResult> GetProductosPaginacionPersonalizada(int pagina = 1)
        {
            if (pagina < 1)
            {
                return BadRequest("El número de página debe ser mayor que 0");
            }
            int registrosPorPagina = 2;
            var productos = await context.Productos.Skip((pagina - 1) * registrosPorPagina).Take(registrosPorPagina).ToListAsync();
            return Ok(productos);
        }

        [HttpGet("seleccioncampos")]
        public async Task<ActionResult> GetProductosSeleccionCampos()
        {
            var productos = await context.Productos.Select(x => new { Id = x.Id, Nombre = x.Nombre }).ToListAsync();
            return Ok(productos);
        }

        [HttpGet("seleccioncamposdtoordenadosnodescatalogados")]
        public async Task<ActionResult> GetProductosSeleccionCamposDTO()
        {
            var productos = await
                context.Productos.Where(x => x.Descatalogado == false)
                .OrderBy(x => x.Nombre)
                .Select(x => new DTOProductosLista { Id = x.Id, NombreProducto = x.Nombre })
                .ToListAsync();

            return Ok(productos);
        }

        [HttpGet("productosagrupadospordescatalogado")]
        public async Task<ActionResult> GetProductosAgrupadosPorDescatalogado()
        {
            var productos = await context.Productos.GroupBy(g => g.Descatalogado)
                .Select(x => new
                {
                    Descatalogado = x.Key,
                    Total = x.Count(),
                    Productos = x.ToList()
                }).ToListAsync();

            return Ok(productos);
        }

        // Filtrado múltiple con mala práctica (ver siguiente get)
        [HttpGet("filtromultiple")]
        public async Task<ActionResult> GetProductosFiltroMultiple([FromQuery] DTOProductosFiltro filtro)
        {
            var productos = await context.Productos.ToListAsync();
            if (filtro.Nombre != "" && filtro.Nombre != null)
            {
                productos = productos.Where(x => x.Nombre.ToLower().Contains(filtro.Nombre.ToLower())).ToList();
            }

            if (filtro.FamiliaId != null && filtro.FamiliaId != 0)
            {
                productos = productos.Where(x => x.FamiliaId == filtro.FamiliaId).ToList();
            }

            productos = productos.Where(x => x.Descatalogado == filtro.Descatalogado).ToList();
            return Ok(productos);
        }

        // Filtrado múltiple como debe hacerse
        [HttpGet("filtromultiple/asqueryable")]
        public async Task<ActionResult> GetProductosFiltroMultipleAsQueryable([FromQuery] DTOProductosFiltro filtro)
        {
            // AsQueryable nos permite ir construyendo paso a paso el filtrado y ejecutarlo al final.
            // Si lo convertimos a una lista (toListAsync) el resto de filtros los hacemos en memoria
            // porque toListAsync ya trae a la memoria del servidor los datos desde el servidor de base de datos
            // Hacer los filtros en memoria es menos eficiente que hacerlos en una base de datos.
            // Construimos los filtros de forma dinámica y hasta que no hacemos el ToListAsync no vamos a la base de datos
            // para traer la información

            var consulta = context.Productos.AsQueryable();
            // select * from productos
            if (filtro.Nombre != "" && filtro.Nombre != null)
            {
                consulta = consulta.Where(x => x.Nombre.ToLower().Contains(filtro.Nombre.ToLower()));
                // select * from productos where nombre like "%pa%
            }

            if (filtro.FamiliaId != null && filtro.FamiliaId != 0)
            {
                consulta = consulta.Where(x => x.FamiliaId == filtro.FamiliaId);
                // select * from productos where nombre like "%pa% && familiaId = 1
            }

            consulta = consulta.Where(x => x.Descatalogado == filtro.Descatalogado);
            // select * from productos where nombre like "%pa% && familiaId = 1 && descatalogado=true
            var productos = await consulta.ToListAsync();

            return Ok(productos);
        }

        [HttpPost]
        public async Task<ActionResult> AgregarProducto(DTOAgregarProducto producto)
        {
            var existeFamilia = await context.Familias.AnyAsync(x => x.Id == producto.FamiliaId);
            if (existeFamilia == false)
            {
                return BadRequest("La familia " + producto.FamiliaId + " no existe");
            }
            var nuevoProducto = new Producto()
            {
                Nombre = producto.Nombre,
                Precio = producto.Precio,
                Descatalogado = producto.Descatalogado,
                FamiliaId = producto.FamiliaId,
                FechaAlta = DateOnly.FromDateTime(DateTime.Now)
            };

            await context.Productos.AddAsync(nuevoProducto);
            await context.SaveChangesAsync();
            return Ok(nuevoProducto);
        }

        [HttpPost("varios")]
        public async Task<ActionResult> AgregarProductos(List<DTOAgregarProducto> productos)
        {
            var listaProductos = new List<Producto>();
            foreach (var p in productos)
            {
                var existeFamilia = await context.Familias.AnyAsync(x => x.Id == p.FamiliaId);
                if (existeFamilia == false)
                {
                    return BadRequest("La familia " + p.FamiliaId + " no existe");
                }
                var nuevoProducto = new Producto()
                {
                    Nombre = p.Nombre,
                    Precio = p.Precio,
                    Descatalogado = p.Descatalogado,
                    FamiliaId = p.FamiliaId,
                    FechaAlta = DateOnly.FromDateTime(DateTime.Now)
                };
                listaProductos.Add(nuevoProducto);
            }

            await context.Productos.AddRangeAsync(listaProductos);
            await context.SaveChangesAsync();
            return Ok();
        }

        [HttpPost("imagen")]
        public async Task<ActionResult> PostProductos([FromForm] DTOAgregarProductoImagen producto)
        {
            Producto newProducto = new Producto
            {
                Nombre = producto.Nombre,
                Precio = producto.Precio,
                Descatalogado = false,
                FechaAlta = DateOnly.FromDateTime(DateTime.Now),
                FamiliaId = producto.FamiliaId,
                FotoUrl = ""
            };

            if (producto.Foto != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    // Extraemos la imagen de la petición
                    await producto.Foto.CopyToAsync(memoryStream);
                    // La convertimos a un array de bytes que es lo que necesita el método de guardar
                    var contenido = memoryStream.ToArray();
                    // Recibimos el nombre del archivo
                    // El servicio Transient GestorArchivosLocal instancia el servicio y cuando se deja de usar se destruye
                    newProducto.FotoUrl = await gestorArchivosService.GuardarArchivo(contenido, producto.Foto.FileName, "imagenes");
                }
            }

            await context.AddAsync(newProducto);
            await context.SaveChangesAsync();
            return Ok(newProducto);
        }

        [HttpPut("imagen")]
        public async Task<ActionResult> PutProductos([FromForm] DTOAgregarProductoImagen producto)
        {
            var productoActualizar = await context.Productos.FindAsync(producto.IdProducto);
            if (productoActualizar == null)
            {
                return NotFound();
            }

            productoActualizar.Nombre = producto.Nombre;
            productoActualizar.Precio = producto.Precio;
            productoActualizar.FamiliaId = producto.FamiliaId;
            productoActualizar.FechaAlta = DateOnly.FromDateTime(DateTime.Now);

            if (producto.Foto != null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    await producto.Foto.CopyToAsync(memoryStream);
                    var contenido = memoryStream.ToArray();
                    productoActualizar.FotoUrl = await gestorArchivosService.EditarArchivo(contenido, producto.Foto.FileName, "imagenes", productoActualizar.FotoUrl);
                }
            }

            await context.SaveChangesAsync();
            return Ok(productoActualizar);
        }

        [HttpPut]
        public async Task<ActionResult> ModificarProducto(DTOAgregarProducto producto)
        {
            // Buscamos si existe el producto que vamos a modificar
            var productoModificar = await context.Productos.FindAsync(producto.Id);
            if (productoModificar == null)
            {
                return NotFound();
            }
            // Chequeamos la familia del producto modificado. Si no existe, devolvemos un error
            var existeFamilia = await context.Familias.AnyAsync(x => x.Id == producto.FamiliaId);
            if (existeFamilia == false)
            {
                return BadRequest("La familia " + producto.FamiliaId + " no existe");
            }

            // Llegados hasta aquí, ya podemos modificar el producto

            productoModificar.Nombre = producto.Nombre;
            productoModificar.Precio = producto.Precio;
            productoModificar.Descatalogado = producto.Descatalogado;
            productoModificar.FamiliaId = producto.FamiliaId;
            productoModificar.FechaAlta = DateOnly.FromDateTime(DateTime.Now);

            await context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> EliminarProducto(int id)
        {
            // Buscamos si existe el producto que vamos a eliminar
            var productoEliminar = await context.Productos.FindAsync(id);
            if (productoEliminar == null)
            {
                return NotFound();
            }

            context.Productos.Remove(productoEliminar);
            await context.SaveChangesAsync();
            return Ok();
        }

        [HttpDelete("imagen/{id}")]
        public async Task<ActionResult> DeleteProductosImagen(int id)
        {
            var producto = await context.Productos.FindAsync(id);
            if (producto == null)
            {
                return NotFound();
            }

            await gestorArchivosService.BorrarArchivo(producto.FotoUrl, "imagenes");
            context.Remove(producto);
            await context.SaveChangesAsync();
            return Ok();
        }


    }
}
