using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SlurkExp.Services.SlurkSetup;

namespace SlurkExp.Pages
{
    public class ResultModel : PageModel
    {
        private readonly SurveyOptions _surveyOptions;
        private readonly ILogger<ResultModel> _logger;

        public ResultModel(
            SurveyOptions surveyOptions,
            ILogger<ResultModel> logger)
        {
            _surveyOptions = surveyOptions;
            _logger = logger;
        }

        public string Type { get; set; }
        public string Token { get; set; }
        public string UrlEnd { get; set; }

        public void OnGet([FromQuery] string type, [FromQuery] string token)
        {
            UrlEnd = _surveyOptions.End;

            Type = type;
            Token = token;
        }
    }
}
