using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SlurkExp.Models
{
    public class Client
    {
        [Key]
        public int ClientId { get; set; }
        public int GroupId { get; set; }
        // Status
        // 0: Created, ready to be dispatched
        // 1: Reserved, sent to qualtrics pre
        // 2: Arrived at qualtrics pre
        // 3: Returned from qualtrics pre, sent to slurk
        // 4: Arrived at slurk
        // 5: Returned from slurk, sent to qualtrics post
        // 6: Arrived at qualtrics post
        // 7: Returned from qualtrics post, sent to prolific
        public int Status { get; set; } = 0;
        public string AccessCode { get; set; } = "";  // Prolific
        public string ClientToken { get; set; } = Guid.NewGuid().ToString("n");
        public string ChatName { get; set; } = "";
        public string ChatToken { get; set; } = "";
        public string ClientJson { get; set; } = "";
        public string Comment { get; set; } = "";
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public DateTime Updated { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Group Group { get; set; }
        [JsonIgnore]
        public ICollection<LogEvent> LogEvents { get; set; } = new List<LogEvent>();
    }
}
