using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace SlurkExp.Pages
{
    [Authorize]
    public class LiveModel : PageModel
    {
        private readonly ILogger<LiveModel> _logger;

        public LiveModel(ILogger<LiveModel> logger)
        {
            _logger = logger;
        }

        public void OnGet()
        {

        }
    }
}
