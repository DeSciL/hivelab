using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SlurkExp.Data;
using SlurkExp.Models;

namespace SlurkExp.Pages.Bots
{
    [Authorize]
    public class DetailModel : PageModel
    {
        private readonly SlurkExpDbContext _context;
        private readonly ILogger<DetailModel> _logger;

        public DetailModel(
            SlurkExpDbContext context,
            ILogger<DetailModel> logger)
        {
            _context = context;
            _logger = logger;
        }

        [BindProperty]
        public Bot Bot { get; set; }

        public async Task<IActionResult> OnGet(int id)
        {
            Bot = await _context.Bots.FirstOrDefaultAsync(x => x.BotId.Equals(id));

            if (Bot == null)
            {
                return RedirectToPage("./index");
            }

            return Page();
        }
    }
}
