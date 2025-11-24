using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SlurkExp.Data.SlurkDb;
using SlurkExp.Services.ApiKey;

namespace SlurkExp.Controllers.WebApi.Slurk
{
    [ApiKey]
    [Route("api/[controller]")]
    [ApiController]
    public class TokensController : ControllerBase
    {
        private readonly SlurkDbContext _context;

        public TokensController(SlurkDbContext context)
        {
            _context = context;
        }

        // GET: api/Tokens
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Token>>> GetTokens()
        {
            return await _context.Tokens.ToListAsync();
        }

        // GET: api/Tokens/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Token>> GetToken(string id)
        {
            var token = await _context.Tokens.FindAsync(id);

            if (token == null)
            {
                return NotFound();
            }

            return token;
        }
    }
}
