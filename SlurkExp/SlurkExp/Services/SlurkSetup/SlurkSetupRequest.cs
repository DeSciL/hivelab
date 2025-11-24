using System.Text.Json.Serialization;

namespace SlurkExp.Services.SlurkSetup
{
    public class SlurkSetupRequest
    {
        [JsonPropertyName("api_token")]
        public string ApiKey { get; set; } = "";
        [JsonPropertyName("num_users")]
        public int UserCount { get; set; } = 0;
        [JsonPropertyName("chatbot_ids")]
        public List<int> BotIds { get; set; } = new List<int>();
        [JsonPropertyName("chatbot_names")]
        public List<string> BotNames { get; set; } = new List<string>();
        [JsonPropertyName("waiting_room_conciergebot_name")]
        public string WaitingRoomManagerName { get; set; } = "Moderator";
        [JsonPropertyName("waiting_room_timeout_url")]
        public string WaitingRoomTimeoutUrl { get; set; } = "";
        [JsonPropertyName("waiting_room_timeout_seconds")]
        public int WaitingRoomTimeoutSeconds { get; set; } = 60;
        [JsonPropertyName("waiting_room_layout_id")]
        public int? WaitingRoomLayoutId { get; set; } = null;
        [JsonPropertyName("min_num_users")]
        public int WaitingRoomMinSize { get; set; } = 2;
        [JsonPropertyName("chat_room_managerbot_name")]
        public string ChatRoomManagerName { get; set; } = "Moderator";
        [JsonPropertyName("chat_room_timeout_url")]
        public string ChatRoomTimeoutUrl { get; set; } = "";
        [JsonPropertyName("chat_room_timeout_seconds")]
        public int ChatRoomTimeoutSeconds { get; set; } = 60;
        [JsonPropertyName("chat_room_dropout_url")]
        public string ChatRoomDropoutUrl { get; set; } = "";
        [JsonPropertyName("chat_room_layout_id")]
        public int? ChatRoomLayoutId { get; set; } = null;
        [JsonPropertyName("min_num_users_chat_room")]
        public int ChatRoomMinSize { get; set; } = 1;
        [JsonPropertyName("user_left_notification_url")]
        public string UserNotificationUrl { get; set; } = "";

        //[JsonPropertyName("proba_ignore_message")]
        //public double BotIgnoreMessage { get; set; } = 0.4;
        //[JsonPropertyName("proba_cancel_message_in_progress")]
        //public double BotCancelMessage { get; set; } = 0.5;
        //[JsonPropertyName("bot_config_json")]
        //public string BotConfig { get; set; } = "";
    }
}
