using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SlurkExp.Data;
using SlurkExp.Models;
using SlurkExp.Services.ApiKey;

namespace SlurkExp.Controllers.WebApi.Exp
{
    [ApiKey]
    [Route("api/[controller]")]
    [ApiController]
    public class LogEventsController : ControllerBase
    {
        private readonly SlurkExpDbContext _context;

        public LogEventsController(SlurkExpDbContext context)
        {
            _context = context;
        }

        // GET: api/LogEvents
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LogEvent>>> GetLogEvents()
        {
            return await _context.LogEvents.ToListAsync();
        }

        // GET: api/LogEvents/5
        [HttpGet("{id}")]
        public async Task<ActionResult<LogEvent>> GetLogEvent(int id)
        {
            var logEvent = await _context.LogEvents.FindAsync(id);

            if (logEvent == null)
            {
                return NotFound();
            }

            return logEvent;
        }

        // PUT: api/LogEvents/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutLogEvent(int id, LogEvent logEvent)
        {
            if (id != logEvent.LogEventId)
            {
                return BadRequest();
            }

            _context.Entry(logEvent).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LogEventExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok(logEvent);
        }

        // POST: api/LogEvents
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<LogEvent>> PostLogEvent(LogEvent logEvent)
        {
            _context.LogEvents.Add(logEvent);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetLogEvent", new { id = logEvent.LogEventId }, logEvent);
        }

        // DELETE: api/LogEvents/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLogEvent(int id)
        {
            var logEvent = await _context.LogEvents.FindAsync(id);
            if (logEvent == null)
            {
                return NotFound();
            }

            _context.LogEvents.Remove(logEvent);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool LogEventExists(int id)
        {
            return _context.LogEvents.Any(e => e.LogEventId == id);
        }
    }
}
