using Microsoft.Extensions.Options;
using SlurkExp.Services.SlurkSetup;

namespace SlurkExp.Services.Settings
{
    public class SettingsService : ISettingsService
    {
        private readonly SlurkSetupOptions _options;
        private readonly ILogger<SettingsService> _logger;

        public SettingsService(
            IOptions<SlurkSetupOptions> options,
            ILogger<SettingsService> logger)
        {
            _options = options.Value;
            _logger = logger;

            WaitingRoomLayoutId = _options.WaitingRoomLayoutId;
            ChatRoomLayoutId = _options.ChatRoomLayoutId;
        }

        public bool IsServerClosed { get; set; } = false;
        public bool IsRandomDispatch { get; set; } = false;

        public int TreatmentOverride { get; set; } = 0;
        public int GroupOverride { get; set; } = 0;
        public int ClientOverride { get; set; } = 0;

        public int WaitingRoomLayoutId { get; set; }
        public int ChatRoomLayoutId { get; set; }
        
    }
}
