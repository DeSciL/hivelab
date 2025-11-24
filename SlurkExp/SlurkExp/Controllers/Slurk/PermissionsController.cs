using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SlurkExp.Data.SlurkDb;
using SlurkExp.Services.ApiKey;

namespace SlurkExp.Controllers.WebApi.Slurk
{
    [ApiKey]
    [Route("api/[controller]")]
    [ApiController]
    public class PermissionsController : ControllerBase
    {
        private readonly SlurkDbContext _context;

        public PermissionsController(SlurkDbContext context)
        {
            _context = context;
        }

        // GET: api/Permissions
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Permission>>> GetPermissions()
        {
            return await _context.Permissions.ToListAsync();
        }

        // GET: api/Permissions/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Permission>> GetPermission(int id)
        {
            var permission = await _context.Permissions.FindAsync(id);

            if (permission == null)
            {
                return NotFound();
            }

            return permission;
        }
    }
}
