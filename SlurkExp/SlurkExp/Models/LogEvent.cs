using System.ComponentModel.DataAnnotations;

namespace SlurkExp.Models
{
    public class LogEvent
    {
        [Key]
        public int LogEventId { get; set; }
        public int ClientId { get; set; } = 0;
        public int Status { get; set; } = 0;
        public string Operation { get; set; } = "";
        public string Data { get; set; } = "";
        public string Comment { get; set; } = "";
        public string LogEventJson { get; set; } = "";
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public DateTime Updated { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Client Client { get; set; }
    }
}
