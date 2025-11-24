using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace SlurkExp.Services.ApiKey
{
    [AttributeUsage(validOn: AttributeTargets.Class | AttributeTargets.Method)]
    public class ApiKeyAttribute : System.Attribute, IAsyncActionFilter
    {
        private const string APIKEYNAME = "X-API-KEY";
        private const string OPTIONSNAME = "ApiKeyOptions";

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!context.HttpContext.Request.Headers.TryGetValue(APIKEYNAME, out var extractedApiKey))
            {
                context.Result = new ContentResult()
                {
                    StatusCode = 401,
                    Content = "Api key was not provided"
                };
                return;
            }

            var _config = context.HttpContext.RequestServices.GetRequiredService<IConfiguration>();
            var _options = _config.GetSection(OPTIONSNAME).Get<ApiKeyOptions>();

            if (!_options.ApiKey.Equals(extractedApiKey))
            {
                context.Result = new ContentResult()
                {
                    StatusCode = 401,
                    Content = "Unauthorized client"
                };
                return;
            }

            await next();
        }
    }
}
