using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SlurkExp.Data;
using SlurkExp.Models;

namespace SlurkExp.Pages.Clients
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
    }
}
