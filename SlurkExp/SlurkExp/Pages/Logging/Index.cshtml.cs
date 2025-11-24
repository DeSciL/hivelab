using cloudscribe.Web.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SlurkExp.Data;
using SlurkExp.Data.SlurkDb;

namespace SlurkExp.Pages.Logging
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly SlurkDbContext _slurkContext;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(
            SlurkExpDbContext context,
            SlurkDbContext slurkContext,
            ILogger<IndexModel> logger)
        {
            _slurkContext = slurkContext;
            _logger = logger;
            Paging = new PaginationSettings();
        }

        public PaginationSettings Paging { get; set; }
        public List<Log> SlurkLogs { get; set; } = new List<Log>();

        public async Task<IActionResult> OnGet([FromRoute] string id, [FromQuery] int? p, [FromQuery] int pageSize = 15)
        {
            var currentPageNum = p.HasValue ? p.Value : 1;
            var offset = (pageSize * currentPageNum) - pageSize;
            Paging.CurrentPage = currentPageNum;
            Paging.ItemsPerPage = pageSize;
            Paging.TotalItems = await _slurkContext.Logs.CountAsync();
            SlurkLogs = await _slurkContext.Logs.OrderByDescending(x => x.Id).Skip(offset).Take(pageSize).ToListAsync();

            return Page();
        }
    }
}
