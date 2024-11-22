using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WebAPIAlmacen.DTOs;

namespace WebAPIAlmacen.Filters
{
    public class FiltroDeExcepcion : ExceptionFilterAttribute
    {
        // Solo en este caso para ir creando un log de errores
        private readonly ILogger<FiltroDeExcepcion> logger;
        private readonly IWebHostEnvironment env;

        public FiltroDeExcepcion(ILogger<FiltroDeExcepcion> logger, IWebHostEnvironment env)
        {
            this.logger = logger;
            this.env = env;
        }

        // El método OnException recibe en un objeto de tipo ExceptionContext 
        // la información de la excepción
        public override void OnException(ExceptionContext context)
        {
            // Si queremos mostrar el error en la terminal
            logger.LogError(context.Exception, context.Exception.Message);

            // Si queremos ir creando un log de errores propio a un archivo"
            var path = $@"{env.ContentRootPath}\wwwroot\logerrores.txt";
            using (StreamWriter writer = new StreamWriter(path, append: true))
            {
                writer.WriteLine(DateTime.Now + " - " + context.Exception.Message);
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
