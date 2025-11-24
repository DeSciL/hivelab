using cloudscribe.Web.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SlurkExp.Data;
using SlurkExp.Models;

namespace SlurkExp.Pages.Clients
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly SlurkExpDbContext _context;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(
            SlurkExpDbContext context,
            ILogger<IndexModel> logger)
        {
            _context = context;
            _logger = logger;
            Paging = new PaginationSettings();
        }

        public PaginationSettings Paging { get; set; }
        public List<Client> Clients = new List<Client>();

        public async Task<IActionResult> OnGet([FromRoute] string id, [FromQuery] int? p, [FromQuery] int pageSize = 15)
        {
            var currentPageNum = p.HasValue ? p.Value : 1;
            var offset = (pageSize * currentPageNum) - pageSize;
            Paging.CurrentPage = currentPageNum;
            Paging.ItemsPerPage = pageSize;
            Paging.TotalItems = await _context.Clients.CountAsync();
            Clients = await _context.Clients.OrderByDescending(x => x.ClientId).Skip(offset).Take(pageSize).ToListAsync();
            return Page();
        }
    }
}
