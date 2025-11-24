using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SlurkExp.Data.SlurkDb;
using SlurkExp.Services.ApiKey;

namespace SlurkExp.Controllers.WebApi.Slurk
{
    [ApiKey]
    [Route("api/[controller]")]
    [ApiController]
    public class LogsController : ControllerBase
    {
        private readonly SlurkDbContext _context;

        public LogsController(SlurkDbContext context)
        {
            _context = context;
        }

        // GET: api/Logs
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Log>>> GetLogs()
        {
            return await _context.Logs.ToListAsync();
        }

        // GET: api/Logs/top/n
        [HttpGet("msg")]
        public async Task<ActionResult<IEnumerable<Log>>> GetMessageLogs([FromQuery] bool msg = false, [FromQuery] int room = 0)
        {
            if(msg && room != 0)
            {
                return await _context.Logs.Where(x => x.Event.Equals("text_message") && x.RoomId == room).ToListAsync();
            }
            if(msg && room == 0)
            {
                return await _context.Logs.Where(x => x.Event.Equals("text_message")).ToListAsync();
            }
            if(!msg && room != 0)
            {
                return await _context.Logs.Where(x => x.RoomId == room).ToListAsync();
            }

            return await _context.Logs.ToListAsync();
        }

        // GET: api/Logs/top/n
        [HttpGet("tail/{n?}")]
        public async Task<ActionResult<IEnumerable<Log>>> GetTopLogs(int n = 10, [FromQuery] bool messagesOnly = false)
        {
            return await _context.Logs.Where(x => x.Event.Equals("text_message")).ToListAsync();
        }

        // GET: api/Logs/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Log>> GetLog(int id)
        {
            var log = await _context.Logs.FindAsync(id);

            if (log == null)
            {
                return NotFound();
            }

            return log;
        }
    }
}
