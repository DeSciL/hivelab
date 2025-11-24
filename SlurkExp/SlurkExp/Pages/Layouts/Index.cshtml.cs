using cloudscribe.Web.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SlurkExp.Data;
using SlurkExp.Data.SlurkDb;

namespace SlurkExp.Pages.Layouts
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
        public List<Layout> SlurkLayouts { get; set; } = new List<Layout>();

        public async Task<IActionResult> OnGet([FromRoute] string id, [FromQuery] int? p, [FromQuery] int pageSize = 15)
        {
            var currentPageNum = p.HasValue ? p.Value : 1;
            var offset = (pageSize * currentPageNum) - pageSize;
            Paging.CurrentPage = currentPageNum;
            Paging.ItemsPerPage = pageSize;
            Paging.TotalItems = await _slurkContext.Layouts.CountAsync();
            SlurkLayouts = await _slurkContext.Layouts.OrderByDescending(x => x.Id).Skip(offset).Take(pageSize).ToListAsync();

            return Page();
        }
    }
}
