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
    public class AuthController : ControllerBase
    {
        private readonly MiAlmacenContext context;
        private readonly TokenService tokenService;
        private readonly HashService hashService;
        public AuthController(MiAlmacenContext context, TokenService tokenService,
            HashService hashService)
        {
            this.context = context;
            this.tokenService = tokenService;
            this.hashService = hashService;
        }

        [HttpPost("login")]
        public async Task<ActionResult> Login([FromBody] DTOUsuario usuario)
        {
            var usuarioDB = await context.Usuarios.FirstOrDefaultAsync(x => x.Email == usuario.Email);
            if (usuarioDB == null)
            {
                return Unauthorized();
            }

            var resultadoHash = hashService.Hash(usuario.Password, usuarioDB.Salt);
            if (usuarioDB.Password == resultadoHash.Hash)
            {
                var respuestaOK = tokenService.GenerarToken(usuario);
                return Ok(respuestaOK);
            }
            else
            {
                return Unauthorized();
            }

        }
    }
}
