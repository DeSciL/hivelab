using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SlurkExp.Data;
using SlurkExp.Data.SlurkDb;
using SlurkExp.Models.ViewModels;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SlurkExp.Pages.ChatLogs
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly SlurkExpDbContext _context;
        private readonly SlurkDbContext _slurkContext;
        private readonly ILogger<IndexModel> _logger;

        public IndexModel(
            SlurkExpDbContext context,
            SlurkDbContext slurkContext,
            ILogger<IndexModel> logger)
        {
            _context = context;
            _slurkContext = slurkContext;
            _logger = logger;
        }

        public List<Log> Logs = new List<Log>();

        public async Task<IActionResult> OnGet(int id)
        {
            Logs = await _slurkContext.Logs.Include(x => x.User)
                .Where(r => r.RoomId.Equals(id))
                .OrderBy(r => r.DateCreated)
                .ToListAsync();

            return Page();
        }

        public string FormatMessage(string json)
        {
            try
            {
                var logMessage = JsonSerializer.Deserialize<LogMessage>(json);
                if (logMessage != null)
                {
                    return logMessage.Message;
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Error deserializing JSON: {Json}", json);
            }
            return json;
        }

        public string FormatEvent(string evt)
        {
            if (evt.StartsWith("join")) return "Join";
            if (evt.StartsWith("text")) return "Text";
            if (evt.StartsWith("leave")) return "Leave";
            return evt;
        }
    }
}
