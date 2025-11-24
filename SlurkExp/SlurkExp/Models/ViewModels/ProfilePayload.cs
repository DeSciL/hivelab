using System.Text.Json.Serialization;

namespace SlurkExp.Models.ViewModels
{
    public class ProfilePayload
    {
        [JsonPropertyName("responseId")]
        public string ResponseId { get; set; } = "";
        [JsonPropertyName("country")]
        public string Country { get; set; } = "";
        [JsonPropertyName("age")]
        public string Age { get; set; } = "";
        [JsonPropertyName("gender")]
        public string Gender { get; set; } = "";
        [JsonPropertyName("education")]
        public string Education { get; set; } = "";
        [JsonPropertyName("profession")]
        public string Profession { get; set; } = "";
        [JsonPropertyName("area")]
        public string Area { get; set; } = "";
        [JsonPropertyName("interests")]
        public string Interests { get; set; } = "";
        [JsonPropertyName("stance")]
        public string Stance { get; set; } = "";
        [JsonPropertyName("name")]
        public string Name { get; set; } = "";
        [JsonPropertyName("camera")]
        public string Camera { get; set; } = "";
        [JsonPropertyName("transport")]
        public string Transport { get; set; } = "";
        [JsonPropertyName("demonstration")]
        public string Demonstration { get; set; } = "";
        [JsonPropertyName("integration")]
        public string Integration { get; set; } = "";
        [JsonPropertyName("topic")]
        public int Topic { get; set; } = 0;
    }
}
