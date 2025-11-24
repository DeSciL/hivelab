using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SlurkExp.Data;
using SlurkExp.Models;

namespace SlurkExp.Pages.Treatments
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
        public Treatment Treatment { get; set; }

        public async Task<IActionResult> OnGet(int id)
        {
            Treatment = await _context.Treatments.FirstOrDefaultAsync(x => x.PromptId.Equals(id));

            if (Treatment == null)
            {
                return RedirectToPage("./index");
            }

            return Page();
        }
    }
}
