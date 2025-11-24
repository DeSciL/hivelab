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
    public class PromptsController : ControllerBase
    {
        private readonly SlurkExpDbContext _context;

        public PromptsController(SlurkExpDbContext context)
        {
            _context = context;
        }

        // GET: api/Prompts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Prompt>>> GetPrompts()
        {
            return await _context.Prompts.ToListAsync();
        }

        // GET: api/Prompts/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Prompt>> GetPrompt(int id)
        {
            var prompt = await _context.Prompts.FindAsync(id);

            if (prompt == null)
            {
                return NotFound();
            }

            return prompt;
        }

        // PUT: api/Prompts/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPrompt(int id, Prompt prompt)
        {
            if (id != prompt.PromptId)
            {
                return BadRequest();
            }

            _context.Entry(prompt).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PromptExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return Ok(prompt);
        }

        // POST: api/Prompts
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Prompt>> PostPrompt(Prompt prompt)
        {
            _context.Prompts.Add(prompt);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetPrompt", new { id = prompt }, prompt);
        }

        // DELETE: api/Prompts/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePrompt(int id)
        {
            var prompt = await _context.Prompts.FindAsync(id);
            if (prompt == null)
            {
                return NotFound();
            }

            _context.Prompts.Remove(prompt);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool PromptExists(int id)
        {
            return _context.Prompts.Any(e => e.PromptId == id);
        }
    }
}
