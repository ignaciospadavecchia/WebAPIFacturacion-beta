namespace WebAPIAlmacen.Middlewares
{
    public class LogMiddleware
    {
        private readonly RequestDelegate next;
        private readonly IWebHostEnvironment env;

        public LogMiddleware(RequestDelegate next, IWebHostEnvironment env)
        {
            this.next = next;
            this.env = env;
        }

        // Invoke o InvokeAsync
        // httpContext contiene la información de la petición. Es lo que necesitamos para poder
        // obtener información de ella y programar el comportamiento del middleware
        public async Task InvokeAsync(HttpContext httpContext)
        {
            var IP = httpContext.Connection.RemoteIpAddress.ToString();
            // Comentamos porque ::1 es localhost y corta todas nuestras peticiones desde swagger
            //if (IP == "::1") // Bloquearía las peticiones de una IP
            //{
            //    httpContext.Response.StatusCode = 400;
            //    httpContext.Response.ContentType = "text/plain";   
            //    await httpContext.Response.WriteAsync("Esta ip tiene restringido el acceso");
            //    return;
            //}
            var ruta = httpContext.Request.Path.ToString();

            var path = $@"{env.ContentRootPath}\wwwroot\log.txt";
            using (StreamWriter writer = new StreamWriter(path, append: true))
            {
                writer.WriteLine($@"{IP} - {ruta}");
            }

            await next(httpContext);
        }
    }

}
