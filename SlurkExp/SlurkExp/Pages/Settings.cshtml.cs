using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SlurkExp.Services.Settings;

namespace SlurkExp.Pages
{
    public class SettingsModel : PageModel
    {
        private readonly ISettingsService _settingsService;
        private readonly ILogger<SettingsModel> _logger;

        public SettingsModel(
            ISettingsService settingsService,
            ILogger<SettingsModel> logger)
        {
            _settingsService = settingsService;
            _logger = logger;
        }

        public class InputModel
        {
            public bool IsServerClosed { get; set; } = false;
            public bool IsRandomDispatch { get; set; } = false;
            //public string A { get; set; } = "1";
            //public string B { get; set; } = "1";
            //public string C { get; set; } = "1";
        }

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();

        public ActionResult OnGet()
        {
            Input.IsServerClosed = _settingsService.IsServerClosed;
            Input.IsRandomDispatch = _settingsService.IsRandomDispatch;
            return Page();
        }

        public IActionResult OnPost()
        {
            _settingsService.IsServerClosed = Input.IsServerClosed;
            _settingsService.IsRandomDispatch = Input.IsRandomDispatch;
            return RedirectToPage("settings");
        }
    }

}
