using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SlurkExp.Data.SlurkDb;
using SlurkExp.Services.ApiKey;

namespace SlurkExp.Controllers.WebApi.Slurk
{
    [ApiKey]
    [Route("api/[controller]")]
    [ApiController]
    public class SessionsController : ControllerBase
    {
        private readonly SlurkDbContext _context;

        public SessionsController(SlurkDbContext context)
        {
            _context = context;
        }

        // GET: api/Sessions
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Session>>> Getsessions()
        {
            return await _context.Sessions.ToListAsync();
        }

        // GET: api/Sessions/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Session>> Getsession(string id)
        {
            var session = await _context.Sessions.FindAsync(id);

            if (session == null)
            {
                return NotFound();
            }

            return session;
        }
    }
}
