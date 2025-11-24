namespace SlurkExp.Models
{
    public class Prompt
    {
        public int PromptId { get; set; }
        public int BotId { get; set; } = 0;
        public string Type { get; set; } = "";
        public string Content { get; set; } = "";
        public string Comment { get; set; } = "";
        public string PromptJson { get; set; } = "";
        public DateTime Created { get; set; } = DateTime.UtcNow;
        public DateTime Updated { get; set; } = DateTime.UtcNow;
    }
}
