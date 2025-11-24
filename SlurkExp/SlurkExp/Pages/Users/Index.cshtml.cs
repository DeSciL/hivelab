using cloudscribe.Web.Pagination;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SlurkExp.Data;
using SlurkExp.Data.SlurkDb;

namespace SlurkExp.Pages.Users
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly SlurkDbContext _slurkContext;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(
            SlurkDbContext slurkContext,
            ILogger<IndexModel> logger)
        {
            _slurkContext = slurkContext;
            _logger = logger;
            Paging = new PaginationSettings();
        }

        public PaginationSettings Paging { get; set; }
        public List<User> SlurkUsers { get; set; } = new List<User>();

        public async Task<IActionResult> OnGet([FromRoute] string id, [FromQuery] int? p, [FromQuery] int pageSize = 15)
        {
            var currentPageNum = p.HasValue ? p.Value : 1;
            var offset = (pageSize * currentPageNum) - pageSize;
            Paging.CurrentPage = currentPageNum;
            Paging.ItemsPerPage = pageSize;
            Paging.TotalItems = await _slurkContext.Users.CountAsync();
            SlurkUsers = await _slurkContext.Users.OrderByDescending(x => x.Id).Skip(offset).Take(pageSize).ToListAsync();

            return Page();
            //var isInt = int.TryParse(id, out var userId);
            //if(isInt)
            //{
            //    SlurkUser = await _slurkContext.Users
            //    .FirstOrDefaultAsync(r => r.Id.Equals(userId));
            //}
            //else
            //{
            //    SlurkUser = await _slurkContext.Users
            //    .FirstOrDefaultAsync(r => r.TokenId.Equals(id));
            //}

            //if (SlurkUser == null) SlurkUser = new SlurkExp.Data.SlurkDb.User();

            // return Page();
        }
    }
}
