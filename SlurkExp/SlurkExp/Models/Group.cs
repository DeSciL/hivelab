using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SlurkExp.Models
{
    public class Group
    {
        [Key]
        public int GroupId { get; set; }
        public int TreatmentId { get; set; }
        public string Name { get; set; } = "";
        public int Status { get; set; } = 0;    // GroupStatus
        public int SortGroup { get; set; } = 0;
        public int SortOrder { get; set; } = 0;
        public int Seats { get; set; } = 0;     // 4 or 5
        public int Overbook { get; set; } = 0;  // 0 or 1
        public int Bots { get; set; } = 0;
        public int WaitingRoomId { get; set; } = 0;
        public int WaitingRoomTime { get; set; } = 0;
        public int ChatRoomId { get; set; } = 0;
        public int ChatRoomTime { get; set; } = 0;
        public int Checkin { get; set; } = 0;
        public int Checkout { get; set; } = 0;
        public string Comment { get; set; } = "";
        public string GroupJson { get; set; } = "";
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public DateTime Updated { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public Treatment Treatment { get; set; }
        [JsonIgnore]
        public ICollection<Client> Clients { get; set; } = new List<Client>();
    }
}
