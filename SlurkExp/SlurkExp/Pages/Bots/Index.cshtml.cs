using cloudscribe.Web.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SlurkExp.Data;
using SlurkExp.Models;
using System.Net.NetworkInformation;

namespace SlurkExp.Pages.Bots
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
        public List<Bot> Bots = new List<Bot>();

        public async Task<IActionResult> OnGet([FromRoute] string id, [FromQuery] int? p, [FromQuery] int pageSize = 15)
        {
            var currentPageNum = p.HasValue ? p.Value : 1;
            var offset = (pageSize * currentPageNum) - pageSize;
            Paging.CurrentPage = currentPageNum;
            Paging.ItemsPerPage = pageSize;
            Paging.TotalItems = await _context.Bots.CountAsync();
            Bots = await _context.Bots.OrderBy(x => x.BotId).Skip(offset).Take(pageSize).ToListAsync();
            return Page();
        }
    }
}
