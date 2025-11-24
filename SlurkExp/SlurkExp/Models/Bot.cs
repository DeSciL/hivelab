namespace SlurkExp.Models
{
    public class Bot
    {
        public int BotId { get; set; }
        public string Name { get; set; } = "";
        public string Type { get; set; } = "";
        public string Comment { get; set; } = "";
        public string BotJson { get; set; } = "";
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public DateTime Updated { get; set; } = DateTime.UtcNow;
    }
}
