using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using SlurkExp.Data;
using SlurkExp.Data.SlurkDb;
using SlurkExp.Models;
using SlurkExp.Models.ViewModels;
using System.Text.Json;

namespace SlurkExp.Pages.Groups
{
    [Authorize]
    public class DetailModel : PageModel
    {
        private readonly SlurkExpDbContext _context;
        private readonly SlurkDbContext _slurkContext;
        private readonly ILogger<DetailModel> _logger;

        public DetailModel(
            SlurkExpDbContext context,
            SlurkDbContext slurkContext,
            ILogger<DetailModel> logger)
        {
            _context = context;
            _slurkContext = slurkContext;
            _logger = logger;
        }

        [BindProperty]
        public Group Group { get; set; }

        public List<Client> Clients { get; set; } = new List<Client>();
        public List<Log> WaitLogs = new List<Log>();
        public List<Log> ChatLogs = new List<Log>();

        public async Task<IActionResult> OnGet(int id)
        {
            Group = await _context.Groups.FirstOrDefaultAsync(x => x.GroupId.Equals(id));

            if (Group != null)
            {
                Clients = await _context.Clients.Where(x => x.GroupId.Equals(Group.GroupId)).ToListAsync();

                if (Group.WaitingRoomId > 0)
                {
                    WaitLogs = await _slurkContext.Logs.Include(x => x.User)
                        .Where(r => r.RoomId.Equals(Group.WaitingRoomId))
                        .OrderBy(r => r.DateCreated)
                        .ToListAsync();
                }

                if (Group.ChatRoomId > 0)
                {
                    ChatLogs = await _slurkContext.Logs.Include(x => x.User)
                        .Where(r => r.RoomId.Equals(Group.ChatRoomId))
                        .OrderBy(r => r.DateCreated)
                        .ToListAsync();
                }
            }

            if (Group == null)
            {
                return RedirectToPage("./index");
            }

            return Page();
        }

        public async Task<IActionResult> OnPostCopyAsync()
        {
            Group newGroup = new Group();

            var g = await _context.Groups.FirstOrDefaultAsync(x => x.GroupId.Equals(Group.GroupId));
            if (g != null)
            {
                newGroup = g;
                newGroup.GroupId = 0;
                newGroup.SortGroup = 0;
                newGroup.SortOrder = 0;
                newGroup.Status = 0;
                newGroup.Checkin = 0;
                newGroup.Checkout = 0;
                newGroup.WaitingRoomId = 0;
                newGroup.WaitingRoomTime = 0;
                newGroup.ChatRoomId = 0;
                newGroup.ChatRoomTime = 0;
                newGroup.Created = DateTime.UtcNow;
                newGroup.Updated = DateTime.UtcNow;
                _context.Groups.Add(newGroup);
                await _context.SaveChangesAsync();
            }

            return RedirectToPage("../groups/detail", new { id = newGroup.GroupId });
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
