using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SlurkExp.Data;
using SlurkExp.Models;

namespace SlurkExp.Pages.Clients
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
        public Client Client { get; set; }

        public async Task<IActionResult> OnGet(int id)
        {
            Client = await _context.Clients.FirstOrDefaultAsync(x => x.ClientId.Equals(id));

            if (Client == null)
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

            _context.Attach(Client).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await _context.Clients.AnyAsync(e => e.ClientId == Client.ClientId))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return RedirectToPage("../clients/edit", new { id = Client.ClientId });
        }
    }

}
