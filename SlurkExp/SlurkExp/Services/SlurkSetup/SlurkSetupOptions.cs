namespace SlurkExp.Services.SlurkSetup
{
    public class SlurkSetupOptions
    {
        public string BaseUrl { get; set; } = "";
        public string ApiKey { get; set; } = "";
        public string WaitingRoomManagerName { get; set; } = "";
        public string WaitingRoomTimeoutUrl { get; set; } = "";
        public int WaitingRoomTimeoutSeconds { get; set; } = 0;
        public int WaitingRoomLayoutId { get; set; } = 0;
        public int WaitingRoomMinSize { get; set; } = 0;
        public string ChatRoomBotIds { get; set; } = "";
        public string ChatRoomBotNames { get; set; } = "";
        public string ChatRoomManagerName { get; set; } = "";
        public string ChatRoomTimeoutUrl { get; set; } = "";
        public int ChatRoomTimeoutSeconds { get; set; } = 0;
        public string ChatRoomDropoutUrl { get; set; } = "";
        public int ChatRoomLayoutId { get; set; } = 0;
        public int ChatRoomMinSize{ get; set; } = 0;
        public double BotIgnoreMessage { get; set; } = 0.5;
        public double BotCancelMessage { get; set; } = 0.5;
        public string BotConfig { get; set; } = "";
        public bool RandomDispatch { get; set; } = false;
        public string UserNotificationUrl { get; set; } = "";
    }
}
