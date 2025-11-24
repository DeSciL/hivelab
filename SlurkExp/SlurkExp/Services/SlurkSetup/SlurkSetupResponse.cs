using System.Text.Json.Serialization;

namespace SlurkExp.Services.SlurkSetup
{
    public class SlurkSetupResponse
    {
        [JsonPropertyName("user_tokens")]
        public List<string> UserTokens { get; set; } = new List<string>();
        [JsonPropertyName("request_id")]
        public string RequestId { get; set; } = "";
        [JsonPropertyName("chat_room_id")]
        public int ChatRoomId { get; set; } = 0;
        [JsonPropertyName("waiting_room_id")]
        public int WaitingRoomId { get; set; } = 0;
    }
}
