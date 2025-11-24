using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SlurkExp.Data;
using SlurkExp.Models;

namespace SlurkExp.Pages.Prompts
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
        public Prompt Prompt { get; set; }

        public async Task<IActionResult> OnGet(int id)
        {
            Prompt = await _context.Prompts.FirstOrDefaultAsync(x => x.PromptId.Equals(id));

            if (Prompt == null)
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

            _context.Attach(Prompt).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Prompts.AnyAsync(e => e.PromptId == Prompt.PromptId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("../prompts/edit", new { id = Prompt.PromptId });
        }
    }

}
