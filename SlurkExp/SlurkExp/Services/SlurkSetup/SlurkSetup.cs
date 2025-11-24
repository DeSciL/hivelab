using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SlurkExp.Data;
using SlurkExp.Models;
using SlurkExp.Models.ViewModels;
using System.Text;
using System.Text.Json;

namespace SlurkExp.Services.SlurkSetup
{
    public class SlurkSetup : ISlurkSetup
    {
        private readonly HttpClient _httpClient;
        private readonly SlurkSetupOptions _options;
        private readonly SlurkExpDbContext _context;
        private readonly ILogger<SlurkSetup> _logger;

        public SlurkSetup(
            HttpClient httpClient,
            IOptions<SlurkSetupOptions> options,
            SlurkExpDbContext context,
            ILogger<SlurkSetup> logger)
        {
            _httpClient = httpClient;
            _options = options.Value;
            _context = context;
            _logger = logger;
            _httpClient.BaseAddress = new Uri(_options.BaseUrl);
        }

        public async Task<SlurkSetupResponse> RoomSetup(int userCount, int minUserCount, List<int> botIDs, List<string> botNames)
        {
            var request = DefaultRequest();

            // Sanitize settings
            if (botIDs.Count > 0)
            {
                request.BotIds = botIDs;
            }
            else
            {
                request.BotIds.Clear();
            }
            if (botNames.Count > 0)
            {
                request.BotNames = botNames;
            }
            else
            {
                request.BotNames.Clear();
            }

            if (_options.WaitingRoomLayoutId == 0) request.WaitingRoomLayoutId = 1;
            if (_options.ChatRoomLayoutId == 0) request.ChatRoomLayoutId = 2;

            request.UserCount = userCount;
            request.WaitingRoomMinSize = minUserCount;
            // TODO: This must be a bug. Should be dynamic.
            request.ChatRoomMinSize = 1;

            // Make reservation request
            var json = JsonSerializer.Serialize(request);
            var contentData = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"setup", contentData);
            var result = await response.Content.ReadFromJsonAsync<SlurkSetupResponse>();

            if(result == null)
            {
                _logger.LogWarning("SlurkSetup.RoomSetup: Failed to get room token.");
            }

            return result;
        }

        public async Task<SlurkSetupResponse> RoomSetup(SlurkSetupRequest request)
        {
            // Make reservation request
            var json = JsonSerializer.Serialize(request);
            var contentData = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"setup", contentData);
            var result = await response.Content.ReadFromJsonAsync<SlurkSetupResponse>();

            if (result == null)
            {
                _logger.LogWarning("SlurkSetup.RoomSetup: Failed to get room token.");
            }

            return result;
        }

        public async Task Assignment(int groupId, int clientId, int treatmentId, SlurkSetupResponse response, List<SlurkLink> slurkLinks, SlurkSetupRequest request)
        {
            Group group;
            if(groupId == 0)
            {
                group = await _context.Groups.FirstOrDefaultAsync(g => g.Status.Equals(0));
                group.Status = 1;
                await _context.SaveChangesAsync();
            }
            else
            {
                group = await _context.Groups.FirstOrDefaultAsync(g => g.GroupId == groupId);
            }

            if(treatmentId > 0)
            {
                group.TreatmentId = treatmentId;
                await _context.SaveChangesAsync();
            }

            // Override with clientId
            string profile = "";
            string comment = "";
            if (clientId > 0)
            {
                var client = await _context.Clients.FirstOrDefaultAsync(c => c.ClientId == clientId);
                if (client != null)
                {
                    profile = client.ClientJson;
                    comment = $"Copied from client: {clientId}";
                }
            }

            if(group != null)
            {
                group.WaitingRoomId = response.WaitingRoomId;
                group.WaitingRoomTime = request.WaitingRoomTimeoutSeconds;
                group.ChatRoomId = response.ChatRoomId;
                group.ChatRoomTime = request.ChatRoomTimeoutSeconds;
                group.Updated = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }

            foreach(var link in slurkLinks)
            {
                _context.Clients.Add( new Client
                {
                    GroupId = group.GroupId,
                    Status = 0,
                    AccessCode = "ManualSetup",
                    ClientToken = Guid.NewGuid().ToString("n"),
                    ChatName = link.Name,
                    ChatToken = link.Token,
                    ClientJson = RenameProfile(profile, link.Name),
                    Comment = comment,
                    Created = DateTime.UtcNow,
                    Updated = DateTime.UtcNow
                });

                await _context.SaveChangesAsync();
            }
        }

        private string RenameProfile(string profile, string name)
        {
            if(string.IsNullOrEmpty(profile)) return "";

            var profilePayload = JsonSerializer.Deserialize<ProfilePayload>(profile);
            profilePayload.Name = name;
            return JsonSerializer.Serialize(profilePayload);
        }

        public SlurkSetupRequest DefaultRequest()
        {
            // TODO: This should be replaced with GetSetupRequest() below
            return new SlurkSetupRequest
            {
                ApiKey = _options.ApiKey,
                UserCount = 1,
                BotIds = _options.ChatRoomBotIds.Split(',').Select(int.Parse).ToList(),
                BotNames = _options.ChatRoomBotNames.Split(',').Select(p => p.Trim()).ToList(),
                WaitingRoomManagerName = _options.WaitingRoomManagerName,
                WaitingRoomTimeoutUrl = _options.WaitingRoomTimeoutUrl,
                WaitingRoomTimeoutSeconds = _options.WaitingRoomTimeoutSeconds,
                WaitingRoomLayoutId = _options.WaitingRoomLayoutId,
                WaitingRoomMinSize = _options.WaitingRoomMinSize,
                ChatRoomManagerName = _options.ChatRoomManagerName,
                ChatRoomTimeoutUrl = _options.ChatRoomTimeoutUrl,
                ChatRoomTimeoutSeconds = _options.ChatRoomTimeoutSeconds,
                ChatRoomDropoutUrl = _options.ChatRoomDropoutUrl,
                ChatRoomLayoutId = _options.ChatRoomLayoutId,
                ChatRoomMinSize = _options.ChatRoomMinSize,
                UserNotificationUrl = _options.UserNotificationUrl
            };
        }

        public SlurkSetupOptions GetSetupOptions()
        {
            return _options;
        }

        public SlurkSetupRequest GetSetupRequest()
        {
            var request = new SlurkSetupRequest
            {
                ApiKey = _options.ApiKey,
                UserCount = 1,
                BotIds = _options.ChatRoomBotIds.Split(',').Select(int.Parse).ToList(),
                BotNames = _options.ChatRoomBotNames.Split(',').Select(p => p.Trim()).ToList(),
                WaitingRoomManagerName = _options.WaitingRoomManagerName,
                WaitingRoomTimeoutUrl = _options.WaitingRoomTimeoutUrl,
                WaitingRoomTimeoutSeconds = _options.WaitingRoomTimeoutSeconds,
                WaitingRoomLayoutId = _options.WaitingRoomLayoutId,
                WaitingRoomMinSize = _options.WaitingRoomMinSize,
                ChatRoomManagerName = _options.ChatRoomManagerName,
                ChatRoomTimeoutUrl = _options.ChatRoomTimeoutUrl,
                ChatRoomTimeoutSeconds = _options.ChatRoomTimeoutSeconds,
                ChatRoomDropoutUrl = _options.ChatRoomDropoutUrl,
                ChatRoomLayoutId = _options.ChatRoomLayoutId,
                ChatRoomMinSize = _options.ChatRoomMinSize,
                UserNotificationUrl = _options.UserNotificationUrl
            };

            return request;
        }
    }
}
