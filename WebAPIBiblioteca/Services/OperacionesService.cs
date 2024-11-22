using Microsoft.EntityFrameworkCore;
using WebAPIBiblioteca.Models;

namespace WebAPIBiblioteca.Services
{
    public class OperacionesService
    {
        private readonly MiBibliotecaContext _context;
        private readonly IHttpContextAccessor _accessor;

        public OperacionesService(MiBibliotecaContext context, IHttpContextAccessor accessor)
        {
            _context = context;
            _accessor = accessor;
        }

        public async Task AddOperacion(string operacion, string controller)
        {
            Operacione nuevaOperacion = new Operacione()
            {
                FechaAccion = DateTime.Now,
                Operacion = operacion,
                Controller = controller,
                Usuario = _accessor.HttpContext.User.Identity.Name,
                Ip = _accessor.HttpContext.Connection.RemoteIpAddress.ToString()
            };
            await _context.Operaciones.AddAsync(nuevaOperacion);
            await _context.SaveChangesAsync();

            await Task.FromResult(0);
        }

        public async Task<bool> PermisoAcceso()
        {
            var ip = _accessor.HttpContext.Connection.RemoteIpAddress.ToString();
            var ultima = await _context.Operaciones
                .Where(x => x.Ip == ip)
                .OrderByDescending(x => x.FechaAccion)
                .FirstOrDefaultAsync();
            if (ultima == null)
            {
                return true;
            }
            else
            {
                var retardo = (DateTime.Now - ultima.FechaAccion).TotalSeconds;
                return retardo > 30;
            }

        }
    }

}
