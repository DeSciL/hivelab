using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SlurkExp.Data;
using SlurkExp.Models;

namespace SlurkExp.Pages.Prompts
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
    }
}