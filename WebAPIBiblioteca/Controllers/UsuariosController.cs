using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPIBiblioteca.DTOs;
using WebAPIBiblioteca.Models;
using WebAPIBiblioteca.Services;

namespace WebAPIBiblioteca.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly MiBibliotecaContext context;
        // Para poder acceder al archivo appSettings con el objetivo de ver la clave de encriptación
        private readonly IConfiguration configuration;
        // Para poder utilizar la clase que .Net tiene para encriptar
        private readonly IDataProtector dataProtector;
        private readonly HashService hashService;
        private readonly IHttpContextAccessor httpContextAccessor;

        public UsuariosController(MiBibliotecaContext context,
            IConfiguration configuration, IDataProtectionProvider dataProtector,
            HashService hashService, IHttpContextAccessor httpContextAccessor)
        {
            this.context = context;
            this.configuration = configuration;
            this.dataProtector = dataProtector.CreateProtector(configuration["ClaveEncriptacion"]);
            this.hashService = hashService;
            this.httpContextAccessor = httpContextAccessor;
        }

        [HttpPost("encriptar/registrar")]
        public async Task<ActionResult> RegisterUsuarioEncrypt([FromBody] DTOUsuario usuario)
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
        public async Task<ActionResult> CheckUsuarioEncrypt([FromBody] DTOUsuario usuario)
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

        [HttpPost("hash/cambiarpassword")]
        public async Task<ActionResult> ChangePassword([FromBody] DTOUsuario usuario)
        {
            var usuarioDB = await context.Usuarios.FirstOrDefaultAsync(x => x.Email == usuario.Email);
            if (usuarioDB == null)
            {
                return Unauthorized();
            }

            var resultadoHash = hashService.Hash(usuario.Password, usuarioDB.Salt);
            if (usuarioDB.Password == resultadoHash.Hash)
            {
                var nuevoHash = hashService.Hash(usuario.NuevoPassword);
                usuarioDB.Password = nuevoHash.Hash;
                usuarioDB.Salt = nuevoHash.Salt;
                await context.SaveChangesAsync();
                return Ok();
            }
            else
            {
                return Unauthorized();
            }
        }

        [HttpPost("linkchangepassword")]
        public async Task<ActionResult> LinkChangePassword([FromBody] string email)
        {
            var usuarioDB = await context.Usuarios.FirstOrDefaultAsync(x => x.Email == email);
            if (usuarioDB == null)
            {
                return Unauthorized("Usuario no registrado");
            }

            // Creamos un string aleatorio 
            Guid miGuid = Guid.NewGuid();
            string textoEnlace = Convert.ToBase64String(miGuid.ToByteArray());
            // Eliminar caracteres que pueden causar problemas
            textoEnlace = textoEnlace.Replace("=", "").Replace("+", "").Replace("/", "").Replace("?", "").Replace("&", "").Replace("!", "").Replace("¡", "");
            usuarioDB.EnlaceCambioPass = textoEnlace;
            usuarioDB.FechaEnvioEnlace = DateTime.Now;
            await context.SaveChangesAsync();
            var ruta = $"{httpContextAccessor.HttpContext.Request.Scheme}://{httpContextAccessor.HttpContext.Request.Host}/changepassword/{textoEnlace}";
            return Ok(ruta);
        }

        [HttpGet("/changepassword/{textoEnlace}")]
        public async Task<ActionResult> LinkChangePasswordHash(string textoEnlace)
        {
            var usuarioDB = await context.Usuarios.FirstOrDefaultAsync(x => x.EnlaceCambioPass == textoEnlace);
            if (usuarioDB == null)
            {
                return Unauthorized("Operación no autorizada");
            }

            var fechaCaducidad = usuarioDB.FechaEnvioEnlace.Value.AddDays(1);

            if (fechaCaducidad < DateTime.Now)
            {
                return Unauthorized("Enlace caducado");
            }

            return Ok("Enlace correcto");
        }

    }
}
