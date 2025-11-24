using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;
using SlurkExp.Services.SlurkSetup;
using System.Reflection.Metadata;

namespace SlurkExp.Pages
{
    public class IframeModel : PageModel
    {
        private readonly SlurkSetupOptions _options;
        private readonly ILogger<IframeModel> _logger;

        public IframeModel(
            IOptions<SlurkSetupOptions> options, 
            ILogger<IframeModel> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public string BaseUrl { get; set; } = "";
        public string Token { get; set; } = "";
        public string Name { get; set; } = "";

        public void OnGet([FromQuery] string token, [FromQuery] string name)
        {
            BaseUrl = _options.BaseUrl;

            if (!string.IsNullOrEmpty(token)) Token = token;
            if (!string.IsNullOrEmpty(name)) Name = name;
            if (!string.IsNullOrEmpty(token) && !string.IsNullOrEmpty(name))
            {
                BaseUrl += $"/login/?token={Token}&name={Name}";
            }
        }
    }
}
