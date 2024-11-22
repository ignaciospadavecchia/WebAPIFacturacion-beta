using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Data;
using WebAPIAlmacen.DTOs;
using WebAPIAlmacen.Models;
using WebAPIAlmacen.Services;

namespace WebAPIAlmacen.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class FamiliasController : ControllerBase
    {
        private readonly MiAlmacenContext context;
        private readonly OperacionesService operacionesService;
        private readonly ContadorPeticionesService contadorPeticionesService;
        private readonly ILogger<FamiliasController> logger;

        // Inyección de dependencia.
        // Como nuestro controller depende de MiAlmacenContext para poder desempeñar sus funciones, lo podemos inyectar en el constructor
        // La inyección de dependencia trae al constructor de la clase todas las dependencias que necesita y las pasa a variables privadas
        // de la clase

        public FamiliasController(MiAlmacenContext context, OperacionesService operacionesService,
            ContadorPeticionesService contadorPeticionesService, ILogger<FamiliasController> logger)
        {
            this.context = context;
            this.operacionesService = operacionesService;
            this.contadorPeticionesService = contadorPeticionesService;
            this.logger = logger;
        }

        // Sin inyección de dependencia (mala práctica)
        //[HttpGet]
        //public List<Familia> GetFamilias()
        //{
        //    var context = new MiAlmacenContext();
        //    return context.Familias.ToList();
        //}

        // Asíncrono vs síncrono
        // Versión síncrona (mala práctica, sobre todo cuando hay muchas peticiones simultáneas al servidor)
        [HttpGet("sincrona")]
        public List<Familia> GetFamiliasVersionSincrona()
        {
            return context.Familias.ToList();
        }

        [HttpGet("adonet")]
        public async Task<ActionResult> GetFamiliasVersionAdonet()
        {
            var connection = new SqlConnection("Data Source=localhost;Initial Catalog=MiAlmacen;Integrated Security=True;TrustServerCertificate=true");
            await connection.OpenAsync();
            var comando = new SqlCommand("select * from familias");
            comando.CommandType = CommandType.Text;
            comando.Connection = connection;
            var familias = await comando.ExecuteReaderAsync();
            var listaFamilias = new List<string>();
            while (familias.Read())
            {
                listaFamilias.Add(familias[1].ToString());
            }
            await connection.CloseAsync();

            return Ok(listaFamilias);
        }

        [HttpGet("asincrona")]
        public async Task<List<Familia>> GetFamiliasVersionAsincrona()
        {
            contadorPeticionesService.Incrementar();
            return await context.Familias.ToListAsync();
        }

        // Mejor versión. Asíncrono con código de respuesta
        [HttpGet("codigorespuesta")]
        public async Task<ActionResult> GetFamiliasVersionCodigoRespuesta()
        {
            contadorPeticionesService.Incrementar();
            var familias = await context.Familias.ToListAsync();
            return Ok(familias);
        }

        // Ruta absoluta (no será lo normal. Posibilidad para peticiones singulares)
        [HttpGet("/todasfamilias")]
        public async Task<ActionResult> GetFamiliasRutaAbsoluta()
        {
            contadorPeticionesService.Incrementar();
            await operacionesService.AddOperacion("Consultar", "Familias");
            var familias = await context.Familias.ToListAsync();
            return Ok(familias);
        }

        // [HttpGet("{id}")] // api/familias/1 -->Si llamamos a api/familias/juan da 400
        [HttpGet("{id:int}")] // api/familias/1 -->Si llamamos a api/familias/juan da 404 por la restricción
        public async Task<ActionResult<Familia>> GetFamiliaPorId(int id)
        {
            logger.LogInformation(id.ToString());
            await operacionesService.AddOperacion("Consultar familias por id " + id, "Familias");
            var familia = await context.Familias.FindAsync(id);
            if (familia == null)
            {
                return NotFound();
            }
            return Ok(familia);
        }

        [HttpGet("nombrecontiene/{texto}")]
        public async Task<ActionResult> GetFamiliasContienenTexto(string texto)
        {
            var familias =
                await context.Familias.Where(x => x.Nombre.Contains(texto)).ToListAsync();

            await operacionesService.AddOperacion("Consultar familias con texto " + texto, "Familias");
            return Ok(familias);
        }

        // Ordenadas por nombre
        [HttpGet("ordenadaspornombre")]
        public async Task<ActionResult> GetFamiliasOrdenadasPorNombre()
        {
            await operacionesService.AddOperacion("Consultar ordenadas", "Familias");
            var familias =
                await context.Familias.OrderBy(x => x.Nombre).ToListAsync();

            return Ok(familias);
        }

        // Ordenadas por nombre ascendente o descendente
        [HttpGet("ordenadaspornombre/{tipoorden}")]
        public async Task<ActionResult> GetFamiliasOrdenadasPorNombreAscDesc(bool tipoorden)
        {
            await operacionesService.AddOperacion("Consultar familias parámetro orden", "Familias");
            if (tipoorden)
            {
                var familias =
                    await context.Familias.OrderBy(x => x.Nombre).ToListAsync();
                return Ok(familias);
            }
            else
            {
                var familias =
                    await context.Familias.OrderByDescending(x => x.Nombre).ToListAsync();
                return Ok(familias);
            }
        }

        [HttpGet("familiasproductos/{id:int}")]
        public async Task<ActionResult<Familia>> GetFamiliasProductosEager(int id)
        {
            // Familia llama a producto y producto a familia, lo que provoca un ciclo infinito del que informa swagger.
            // Por eso, hay que ir al Program y el la configuración de los controllers determinar que se ignoren los ciclos
            // Con ThenInclude podemos profundizar más en las relaciones
            var familia = await context.Familias.Include(x => x.Productos).FirstOrDefaultAsync(x => x.Id == id);
            if (familia == null)
            {
                return NotFound();
            }
            await operacionesService.AddOperacion("Consultar familia " + id + " y sus productos", "Familias");
            return Ok(familia);
        }

        [HttpGet("todasfamiliasproductos")]
        public async Task<ActionResult<Familia>> GetTodasFamiliasProductosEager()
        {
            var familia = await context.Familias.Include(x => x.Productos).ToListAsync();
            if (familia == null)
            {
                return NotFound();
            }
            return Ok(familia);
        }

        [HttpGet("autoreslibrosselect/{id:int}")]
        public async Task<ActionResult<Familia>> GetFamiliasProductosSelect(int id)
        {
            var familia = await context.Familias
                       .Select(x => new DTOFamiliaProducto
                       {
                           IdFamilia = x.Id,
                           Nombre = x.Nombre,
                           TotalProductos = x.Productos.Count(),
                           PrecioPromedio = x.Productos.Average(x => x.Precio),
                           Productos = x.Productos.Select(y => new DTOProductoItem
                           {
                               IdProducto = y.Id,
                               Nombre = y.Nombre
                           }).ToList(),
                       }).FirstOrDefaultAsync(x => x.IdFamilia == id);
            if (familia == null)
            {
                return NotFound();
            }
            return Ok(familia);
        }

        [HttpGet("sql/{id:int}")]
        public async Task<ActionResult<Familia>> FamiliaPorIdSQL(int id)
        {
            var familia = await context.Familias
                        .FromSqlInterpolated($"SELECT * FROM Familias WHERE Id = {id}")
                        .FirstOrDefaultAsync();

            if (familia == null)
            {
                return NotFound();
            }
            return Ok(familia);
        }

        [HttpPost("sql")]
        public async Task<ActionResult> Post(DTOAgregarFamilia familia)
        {
            //Ejemplo de sentencia SQL de inserción	
            await context.Database
                .ExecuteSqlInterpolatedAsync($@"INSERT INTO Familias(Nombre) VALUES({familia.Nombre})");

            return Ok();
        }

    }
}
