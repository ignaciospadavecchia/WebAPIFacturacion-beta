using Microsoft.AspNetCore.DataProtection;
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
    public class UsuariosController : ControllerBase
    {
        private readonly MiAlmacenContext context;
        // Para poder acceder al archivo appSettings con el objetivo de ver la clave de encriptación
        private readonly IConfiguration configuration;
        // Para poder utilizar la clase que .Net tiene para encriptar
        private readonly IDataProtector dataProtector;
        // Debemos inyectar el servicio HashService para poder encriptar
        // las contraseñas. El servicio está en Program registrado como Transient
        private readonly HashService hashService;

        public UsuariosController(MiAlmacenContext context, 
            IConfiguration configuration, IDataProtectionProvider dataProtector,
            HashService hashService)
        {
            this.context = context;
            this.configuration = configuration;
            this.dataProtector = dataProtector.CreateProtector(configuration["ClaveEncriptacion"]);
            this.hashService = hashService;
        }

        [HttpPost("encriptar/registrar")]
        public async Task<ActionResult> PostNuevoUsuario([FromBody] DTOUsuario usuario)
        {
            // dataProtector.Protect encripta el valor entre ()
            var passEncriptado = dataProtector.Protect(usuario.Password);
            var newUsuario = new Usuario
            {
                Email = usuario.Email,
                Password = passEncriptado
            };
            await context.Usuarios.AddAsync(newUsuario);
            await context.SaveChangesAsync();

            return Ok(newUsuario);
        }

        [HttpPost("encriptar/iniciarsesion")]
        public async Task<ActionResult> PostCheckUserPassEncriptado([FromBody] DTOUsuario usuario)
        {
            var usuarioDB = await context.Usuarios.FirstOrDefaultAsync(x => x.Email == usuario.Email);
            if (usuarioDB == null)
            {
                return Unauthorized();
            }

            var passDesencriptado = dataProtector.Unprotect(usuarioDB.Password);
            if (usuario.Password == passDesencriptado)
            {
                return Ok();
            }
            else
            {
                return Unauthorized();
            }
        }

        [HttpPost("hash/registrar")]
        public async Task<ActionResult> PostNuevoUsuarioHash([FromBody] DTOUsuario usuario)
        {
            var resultadoHash = hashService.Hash(usuario.Password);
            var newUsuario = new Usuario
            {
                Email = usuario.Email,
                Password = resultadoHash.Hash,
                Salt = resultadoHash.Salt
            };

            await context.Usuarios.AddAsync(newUsuario);
            await context.SaveChangesAsync();

            return Ok(newUsuario);
        }

        [HttpPost("hash/iniciarsesion")]
        public async Task<ActionResult> CheckUsuarioHash([FromBody] DTOUsuario usuario)
        {
            var usuarioDB = await context.Usuarios.FirstOrDefaultAsync(x => x.Email == usuario.Email);
            if (usuarioDB == null)
            {
                return Unauthorized();
            }

            var resultadoHash = hashService.Hash(usuario.Password, usuarioDB.Salt);
            if (usuarioDB.Password == resultadoHash.Hash)
            {
                return Ok();
            }
            else
            {
                return Unauthorized();
            }

        }

    }
}
