using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPIBiblioteca.Models;

namespace WebAPIBiblioteca.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OperacionesController : ControllerBase
    {
        private readonly MiBibliotecaContext context;

        public OperacionesController(MiBibliotecaContext context)
        {
            this.context = context;
        }

        [HttpGet]
        public async Task<ActionResult> GetOperaciones()
        {
            var operaciones = await context.Operaciones.ToListAsync();
            return Ok(operaciones);
        }
    }
}
