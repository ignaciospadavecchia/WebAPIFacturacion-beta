using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebAPIAlmacen.Models;
using WebAPIAlmacen.Services;

namespace WebAPIAlmacen.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogController : ControllerBase
    {
        private readonly MiAlmacenContext context;

        public LogController(MiAlmacenContext context)
        {
            this.context = context;
        }

        [HttpGet]
        public async Task<ActionResult> GetLogs()
        {
            var logs = await context.Logs.ToListAsync();
            return Ok(logs);
        }
    }
}
