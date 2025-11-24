using System.Text.Json.Serialization;

namespace SlurkExp.Models.ViewModels
{
    public class LogMessage
    {
        [JsonPropertyName("message")]
        public string Message { get; set; }
        [JsonPropertyName("html")]
        public bool Html { get; set; }
        [JsonPropertyName("broadcast")]
        public bool Broadcast { get; set; }
    }
}
