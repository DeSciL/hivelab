using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SlurkExp.Data;
using SlurkExp.Models;

namespace SlurkExp.Pages.Bots
{
    [Authorize]
    public class EditModel : PageModel
    {
        private readonly SlurkExpDbContext _context;
        private readonly ILogger<EditModel> _logger;

        public EditModel(
            SlurkExpDbContext context,
            ILogger<EditModel> logger)
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

        public async Task<IActionResult> OnPostAsync()
        {

            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(Bot).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Bots.AnyAsync(e => e.BotId == Bot.BotId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("../bots/edit", new { id = Bot.BotId });
        }
    }

}
