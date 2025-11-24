using cloudscribe.Web.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SlurkExp.Data;
using SlurkExp.Models;

namespace SlurkExp.Pages.LogEvents
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
        public List<LogEvent> LogEvents = new List<LogEvent>();
        public int Refresh { get; set; } = 0;
        

        public async Task<IActionResult> OnGet([FromRoute] string id, [FromQuery] int? p, [FromQuery] int pageSize = 15, [FromQuery] int refresh = 0)
        {
            var currentPageNum = p.HasValue ? p.Value : 1;
            var offset = (pageSize * currentPageNum) - pageSize;
            Paging.CurrentPage = currentPageNum;
            Paging.ItemsPerPage = pageSize;
            Paging.TotalItems = await _context.LogEvents.CountAsync();
            LogEvents = await _context.LogEvents.OrderByDescending(x => x.LogEventId).Skip(offset).Take(pageSize).ToListAsync();

            if(refresh > 0) Refresh = refresh;

            return Page();
        }
    }
}
