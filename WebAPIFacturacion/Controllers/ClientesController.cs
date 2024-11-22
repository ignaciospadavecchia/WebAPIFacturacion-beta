using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPIFacturacion.DTO;
using WebAPIFacturacion.Models;

namespace WebAPIFacturacion.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ClientesController : ControllerBase
    {
        private readonly MiFacturacionContext context;

        public ClientesController(MiFacturacionContext context)
        {
            this.context = context;
        }

        // Devolver todos los clientes.
        [HttpGet("getClientes")]
        public async Task<ActionResult<List<Cliente>>> GetClientes()
        {
            return Ok(await context.Clientes.ToListAsync());
        }

        // Devolver	todos los clientes de una ciudad que se pasará en la ruta.
        [HttpGet("getClientesPorCiudad")]
        public async Task<ActionResult<List<Cliente>>> GetClientesPorCiudad(string ciudad)
        {
            var clienteCiudad = await context.Clientes.Where(c => c.Ciudad == ciudad).ToListAsync();
            return Ok(clienteCiudad);
        }

        // Agregar	un cliente.
        [HttpPost("crearCliente")]
        public async Task<ActionResult<List<Cliente>>> CrearCliente([FromBody] DTOCliente cliente)
        {
            var nuevoCliente = new Cliente()
            {
                Nombre = cliente.Nombre,
                Ciudad = cliente.Ciudad
            };

            await context.Clientes.AddAsync(nuevoCliente);
            await context.SaveChangesAsync();
            return Ok(nuevoCliente);
        }

        // Modificar un cliente.
        [HttpPut("modificarCliente")]
        public async Task<ActionResult<List<Cliente>>> ModificarCliente([FromBody] DTOCliente cliente)
        {
            // Buscar si existe el cliente 
            var clienteExistente = await context.Clientes.FindAsync(cliente.IdCliente);

            if (clienteExistente == null)
            {
                return NotFound();
            }

            // Modificar 
            clienteExistente.Nombre = cliente.Nombre;
            clienteExistente.Ciudad = cliente.Ciudad;

            await context.SaveChangesAsync();

            //HTTP 204 para indicar que la operación se completó con éxito pero no hay contenido que devolver en la respuesta.
            return NoContent();
        }

        // Eliminar	un cliente. Verificar antes que no haya facturas de ese cliente.
        [HttpDelete("{idCliente}")]
        public async Task<ActionResult<List<Cliente>>> EliminarCliente(int idCliente)
        {
            // Verificar que exista el cliente que vamos a eliminar
            var clienteEliminar = await context.Clientes.FindAsync(idCliente);
            if (clienteEliminar == null)
            {
                return NotFound();
            }
            // Verificar que el cliente no tenga facturas a pagar
            var clienteTieneFacturasPendientes = await context.Facturas.AnyAsync(factura => factura.IdCliente == idCliente && !factura.Pagada);

            if (clienteTieneFacturasPendientes)
            {
                return BadRequest("El cliente que intenta eliminar tiene facturas pendientes de pagar. Por favor, primero salde las facturas pendientes.");               
            }
            else
            {
                context.Clientes.Remove(clienteEliminar);
                await context.SaveChangesAsync();
                return NoContent();
            }
        }

        // Devolver todas las facturas.
        [HttpGet("getFacturas")]
        public async Task <ActionResult<List<Cliente>>> GetFacturas()
        {
            var facturas = await context.Facturas.ToListAsync();
            return Ok(facturas);
        }

        // Devolver	la factura que corresponda a un número concreto que se pasará en	la ruta.
        

        // Devolver	las facturas cuyo importe supere uno que se pasará en la ruta.

        // Devolver	las facturas pagadas.

        // Devolver	las facturas de un cliente que se pasará en la ruta.

        // Agregar	una factura.

        // Modificar	una factura.

        // Eliminar	una factura.

        // Obtener	una lista de clientes con la siguiente información construyendo	una clase DTO	a	medida
    }
}
