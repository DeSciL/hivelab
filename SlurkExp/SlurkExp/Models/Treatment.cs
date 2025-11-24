using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace SlurkExp.Models
{
    public class Treatment
    {
        [Key]
        public int TreatmentId { get; set; }
        public string Name { get; set; } = "";
        public int Category { get; set; } = 0;      // {0,1,2,3,4,5}
        public int SortOrder { get; set; } = 0;     // Random 1-10000
        public int GroupCount { get; set; } = 0;    // {4, 32}
        public int Seats { get; set; } = 0;         // {4, 5}
        public int Overbook { get; set; } = 0;      // {0, 1, 2, 3}
        public int Bots { get; set; } = 0;          // {0, 1, 2, 3}
        public int PromptId { get; set; } = 0;
        public int Info { get; set; } = 0;          // {0,1}
        public int Positive { get; set; } = 0;      // {0,1}
        public int Topic { get; set; } = 0;         // {1,2,3,4}
        public string Comment { get; set; } = "";
        public string TreatmentJson { get; set; } = "";
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public DateTime Updated { get; set; } = DateTime.UtcNow;

        // Navigation properties
        [JsonIgnore]
        public ICollection<Group> Groups { get; set; } = new List<Group>();
    }
}
