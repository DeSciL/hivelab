using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using SlurkExp.Data;
using SlurkExp.Services.Hub;
using SlurkExp.Services.Settings;
using SlurkExp.Services.SlurkSetup;
using System.Text.Json;

namespace SlurkExp.Pages
{
    [Authorize]
    public class SetupModel : PageModel
    {
        private readonly ISlurkSetup _slurkSetup;
        private readonly IHubService _hubService;
        private readonly ISlurkExpRepository _repo;
        private readonly ISettingsService _settingsService;
        private readonly ILogger<SetupModel> _logger;

        public SetupModel(
            ISlurkSetup slurkSetup,
            IHubService hubService,
            ISlurkExpRepository repo,
            ISettingsService settingsService,
            ILogger<SetupModel> logger)
        {
            _slurkSetup = slurkSetup;
            _hubService = hubService;
            _repo = repo;
            _settingsService = settingsService;
            _logger = logger;

            Input = new InputModel();
            Input.WaitingRoomLayoutId = _settingsService.WaitingRoomLayoutId;
            Input.ChatRoomLayoutId = _settingsService.ChatRoomLayoutId;
        }

        public class InputModel
        {
            public int UserCount { get; set; } = 1;
            public string BotId { get; set; } = "1";
            public string BotName { get; set; } = "Ash";

            public int TreatmentOverride { get; set; } = 0;
            public int GroupOverride { get; set; } = 0;
            public int ClientOverride { get; set; } = 0;

            public string WaitingRoomManagerName { get; set; } = "Moderator";
            public int WaitingRoomTimeoutSeconds { get; set; } = 180;
            public string WaitingRoomTimeoutUrl { get; set; } = "/return/wait";
            public int WaitingRoomLayoutId { get; set; } = 1;
            public int WaitingRoomMinSize { get; set; } = 1;

            public string ChatRoomManagerName { get; set; } = "Moderator";
            public int ChatRoomTimeoutSeconds { get; set; } = 180;
            public string ChatRoomTimeoutUrl { get; set; } = "/return/chat";
            public string ChatRoomDropoutUrl { get; set; } = "/return/drop";
            public int ChatRoomLayoutId { get; set; } = 2;
            public int ChatRoomMinSize { get; set; } = 1;

            public double BotIgnoreMessage { get; set; } = 0.4;
            public double BotCancelMessage { get; set; } = 0.5;
            public string BotConfig { get; set; } = "";
        }

        [BindProperty]
        public InputModel Input { get; set; } = new InputModel();
        public SlurkSetupResponse SlurkResponse { get; set; } = new SlurkSetupResponse();

        public List<SlurkLink> SlurkLinks = new List<SlurkLink>();

        public IActionResult OnGetAsync()
        {
            // TODO: Room Override
            Input.TreatmentOverride = _settingsService.TreatmentOverride;
            Input.GroupOverride = _settingsService.GroupOverride;
            Input.ClientOverride = _settingsService.ClientOverride;
            Input.WaitingRoomLayoutId = _settingsService.WaitingRoomLayoutId;
            Input.ChatRoomLayoutId = _settingsService.ChatRoomLayoutId;

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // TODO: Save overrides
            if (Input.TreatmentOverride > 0) _settingsService.TreatmentOverride = Input.TreatmentOverride;
            if (Input.GroupOverride > 0) _settingsService.GroupOverride = Input.GroupOverride;
            if (Input.ClientOverride > 0) _settingsService.ClientOverride = Input.ClientOverride;

            _settingsService.WaitingRoomLayoutId = Input.WaitingRoomLayoutId;
            _settingsService.ChatRoomLayoutId = Input.ChatRoomLayoutId;
            
            var request = _slurkSetup.DefaultRequest();
            request.UserCount = Input.UserCount;

            if(string.IsNullOrEmpty(Input.BotId))
            {
                request.BotIds = new List<int>();
            }
            else if(Input.BotId.Contains(','))
            {
                var ids = Input.BotId.Split(',');
                request.BotIds.Clear();
                foreach (var id in ids) request.BotIds.Add(Convert.ToInt32(id));
            }
            else
            {
                request.BotIds = new List<int> { Convert.ToInt32(Input.BotId) };
            }

            if(string.IsNullOrEmpty(Input.BotName))
            {
                request.BotNames = new List<string>();
            }
            else if (Input.BotName.Contains(','))
            {
                var names = Input.BotName.Split(',');
                request.BotNames.Clear();
                foreach (var name in names) request.BotNames.Add(name);
            }
            else
            {
                request.BotNames = new List<string> { Input.BotName };
            }

            //request.WaitingRoomManagerName = Input.WaitingRoomManagerName;
            request.WaitingRoomTimeoutSeconds = Input.WaitingRoomTimeoutSeconds;
            //request.WaitingRoomTimeoutUrl = Input.WaitingRoomTimeoutUrl;
            request.WaitingRoomLayoutId = Input.WaitingRoomLayoutId;
            request.WaitingRoomMinSize = Input.WaitingRoomMinSize;

            //request.ChatRoomManagerName = Input.ChatRoomManagerName;
            request.ChatRoomTimeoutSeconds = Input.ChatRoomTimeoutSeconds;
            //request.ChatRoomTimeoutUrl = Input.ChatRoomTimeoutUrl;
            //request.ChatRoomDropoutUrl = Input.ChatRoomDropoutUrl;
            request.ChatRoomLayoutId = Input.ChatRoomLayoutId;
            request.ChatRoomMinSize = Input.ChatRoomMinSize;

            // TODO
            //request.BotIgnoreMessage = Input.BotIgnoreMessage;
            //request.BotCancelMessage = Input.BotCancelMessage;
            
            // Get UserToken, ChatRoomId, and WaitingRoomId from Slurk
            SlurkResponse = await _slurkSetup.RoomSetup(request);
            
            var endpoint = _slurkSetup.GetSetupOptions().BaseUrl;
            foreach (var token in SlurkResponse.UserTokens)
            {
                var name = Faker.Name.First();
                var link = new SlurkLink
                {
                    Token = token,
                    Name = name,
                    Url = $"{endpoint}/login/?token={token}&name={name}"
                };

                SlurkLinks.Add(link);
            }

            // Save WaitingRoomId and ChatRoomId to group, create client objects
            await _slurkSetup.Assignment(Input.GroupOverride, Input.ClientOverride, Input.TreatmentOverride, SlurkResponse, SlurkLinks, request);

            // Save to Log
            await Log(1, "Setup", $"Created waiting room {SlurkResponse.WaitingRoomId} and chat room {SlurkResponse.ChatRoomId}.");

            return Page();
        }

        /// <summary>
        /// Signal helper
        /// </summary>
        private async Task Signal(string operation, string data)
        {
            await _hubService.SignalAgentHub($"{operation}: {data}");
        }

        /// <summary>
        /// Logging helper
        /// </summary>
        private async Task Log(int clientId, string operation, string data)
        {
            await _hubService.SignalAgentHub($"{operation}: {clientId}: {data}");
            await _repo.AddLogEvent(clientId, operation, data);
        }
    }
}
