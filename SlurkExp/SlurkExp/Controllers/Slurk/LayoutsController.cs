using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SlurkExp.Data.SlurkDb;
using SlurkExp.Services.ApiKey;

namespace SlurkExp.Controllers.WebApi.Slurk
{
    [ApiKey]
    [Route("api/[controller]")]
    [ApiController]
    public class LayoutsController : ControllerBase
    {
        private readonly SlurkDbContext _context;

        public LayoutsController(SlurkDbContext context)
        {
            _context = context;
        }

        // GET: api/Layouts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Layout>>> GetLayouts()
        {
            return await _context.Layouts.ToListAsync();
        }

        // GET: api/Layouts/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Layout>> GetLayout(int id)
        {
            var layout = await _context.Layouts.FindAsync(id);

            if (layout == null)
            {
                return NotFound();
            }

            return layout;
        }
    }
}
