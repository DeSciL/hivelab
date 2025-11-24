using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SlurkExp.Data;
using SlurkExp.Models;

namespace SlurkExp.Pages.Treatments
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

        public async Task<IActionResult> OnPostAsync()
        {

            if (!ModelState.IsValid)
            {
                return Page();
            }

            _context.Attach(Treatment).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Treatments.AnyAsync(e => e.TreatmentId == Treatment.TreatmentId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("../treatments/edit", new { id = Treatment.TreatmentId });
        }
    }

}
