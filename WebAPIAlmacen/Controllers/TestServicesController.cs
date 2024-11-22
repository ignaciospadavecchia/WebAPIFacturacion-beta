using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebAPIAlmacen.Services;
using WebAPIAlmacen.Services.Demo;

namespace WebAPIAlmacen.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestServicesController : ControllerBase
    {
        private readonly TransientService transientService;
        private readonly ScopedService scopedService;
        private readonly SingletonService singletonService;
        private readonly TestService testService;
        private readonly ContadorPeticionesService contadorPeticionesService;
        public TestServicesController(TransientService transientService, ScopedService scopedService, SingletonService singletonService, TestService testService,
            ContadorPeticionesService contadorPeticionesService)
        {
            this.transientService = transientService;
            this.scopedService = scopedService;
            this.singletonService = singletonService;
            this.testService = testService;
            this.contadorPeticionesService = contadorPeticionesService;
        }

        [HttpGet]
        public ActionResult GetGuid()
        {
            var guidTransient = transientService.Guid;
            var guidTransient2 = testService.GetTransient();
            var guidScoped = scopedService.Guid;
            var guidScoped2 = testService.GetScoped();
            var guidSingleton = singletonService.Guid;
            var guidSingleton2 = testService.GetSingleton();
            return Ok(new
            {
                servicioTransient = guidTransient,
                servicioTransient2 = guidTransient2,
                servicioScoped = guidScoped,
                servicioScoped2 = guidScoped2,
                servicioSingleton = guidSingleton,
                servicioSingleton2 = guidSingleton2
            });
        }

        [HttpGet("contador")]
        public ActionResult GetContador()
        {
            return Ok(contadorPeticionesService.GetContador());
        }

    }
}
