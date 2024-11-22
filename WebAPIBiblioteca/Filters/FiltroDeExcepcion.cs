using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WebAPIBiblioteca.DTOs;

namespace WebAPIBiblioteca.Filters
{
    public class FiltroDeExcepcion : ExceptionFilterAttribute
    {
        private readonly IWebHostEnvironment env;

        public FiltroDeExcepcion(IWebHostEnvironment env)
        {
            this.env = env;
        }

        public override void OnException(ExceptionContext context)
        {
            // Si queremos ir creando un log de errores propio a un archivo"
            var path = $@"{env.ContentRootPath}\wwwroot\logerrores.txt";
            using (StreamWriter writer = new StreamWriter(path, append: true))
            {
                writer.WriteLine(DateTime.Now + " - " + 
                    context.HttpContext.Connection.RemoteIpAddress + " - " +
                    context.HttpContext.Request.Path + " - " +
                    context.HttpContext.Request.Method + " - " +
                    context.Exception.Message);
            }

            // Importante: Devolvemos una respuesta uniforme en todos los errores no controlados
            // Construimos la respuesta
            var errorRespuesta = new DTOError()
            {
                Mensaje = context.Exception.Message,
                Error = context.Exception.ToString()
            };
            // La pasamos a JSON para que el front la pueda manipular
            context.Result = new JsonResult(errorRespuesta);
            // Devuelve la excepción
            base.OnException(context);
        }

    }
}
