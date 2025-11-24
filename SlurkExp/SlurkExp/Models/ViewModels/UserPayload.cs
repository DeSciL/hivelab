using System.Text.Json.Serialization;

namespace SlurkExp.Models.ViewModels
{
    public class UserPayload
    {
        [JsonPropertyName("user_id")]
        public int UserId { get; set; }
        [JsonPropertyName("chat_room_id")]
        public int RoomId { get; set; }
    }
}
