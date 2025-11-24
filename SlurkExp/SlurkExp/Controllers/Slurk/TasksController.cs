using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SlurkExp.Data.SlurkDb;
using SlurkExp.Services.ApiKey;

namespace SlurkExp.Controllers.WebApi.Slurk
{
    [ApiKey]
    [Route("api/[controller]")]
    [ApiController]
    public class TasksController : ControllerBase
    {
        private readonly SlurkDbContext _context;

        public TasksController(SlurkDbContext context)
        {
            _context = context;
        }

        // GET: api/Tasks
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Data.SlurkDb.Task>>> GetLogs()
        {
            return await _context.Tasks.ToListAsync();
        }

        // GET: api/Tasks/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Data.SlurkDb.Task>> GetLog(int id)
        {
            var task = await _context.Tasks.FindAsync(id);

            if (task == null)
            {
                return NotFound();
            }

            return task;
        }
    }
}
