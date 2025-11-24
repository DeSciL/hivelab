using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace SlurkExp.Controllers
{
    [ApiController]
    public class AppController : ControllerBase
    {
        private IConfiguration _config;
        private readonly ILogger<AppController> _logger;

        public AppController(
            IConfiguration config,
            ILogger<AppController> logger)
        {
            _config = config;
            _logger = logger;
        }

        [HttpGet("~/api/app")]
        public IActionResult GetApp()
        {
            var res = new
            {
                Host = Environment.MachineName,
                Version = Assembly.GetExecutingAssembly().GetName().Version,
                LogLevel = _config.GetValue<string>("Serilog:MinimumLevel:Default")
            };

            return new JsonResult(res);
        }

        [HttpGet("~/api/app/host")]
        public IActionResult GetHost()
        {
            return new JsonResult(Environment.MachineName);
        }

        [HttpGet("~/api/app/version")]
        public IActionResult GetVersion()
        {
            return new JsonResult(Assembly.GetExecutingAssembly().GetName().Version);
        }

        [HttpGet("~/api/app/loglevel")]
        public IActionResult GetConfig()
        {
            var logLevel = _config.GetValue<string>("Serilog:MinimumLevel:Default");
            return new JsonResult(logLevel);
        }
    }
}
