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
    public class BotsController : ControllerBase
    {
        private readonly SlurkExpDbContext _context;

        public BotsController(SlurkExpDbContext context)
        {
            _context = context;
        }

        // GET: api/bots
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Bot>>> GetBots()
        {
            return await _context.Bots.ToListAsync();
        }

        // GET: api/bots/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Bot>> GetBot(int id)
        {
            var bot = await _context.Bots.FindAsync(id);

            if (bot == null)
            {
                return NotFound();
            }

            return bot;
        }

        // PUT: api/bots/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutBot(int id, Bot bot)
        {
            if (id != bot.BotId)
            {
                return BadRequest();
            }

            _context.Entry(bot).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BotExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok(bot);
        }

        // POST: api/bots
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Bot>> PostClient(Bot bot)
        {
            _context.Bots.Add(bot);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetBot", new { id = bot.BotId }, bot);
        }

        // DELETE: api/bots/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBot(int id)
        {
            var bot = await _context.Bots.FindAsync(id);
            if (bot == null)
            {
                return NotFound();
            }

            _context.Bots.Remove(bot);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool BotExists(int id)
        {
            return _context.Bots.Any(e => e.BotId == id);
        }
    }
}
