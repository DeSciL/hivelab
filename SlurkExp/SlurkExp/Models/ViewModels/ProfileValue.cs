namespace SlurkExp.Models.ViewModels
{
    public class ProfileValue
    {
        public string Country { get; set; } = "";
        public string Age { get; set; } = "";
        public string Gender { get; set; } = "";
        public string Education { get; set; } = "";
        public string Profession { get; set; } = "";
        public string Area { get; set; } = "";
        public string Stance { get; set; } = "";
        public string Name { get; set; } = "";
        public string Interests { get; set; } = "";
        public List<int> Camera { get; set; } = new List<int>();
        public List<int> Transport { get; set; } = new List<int>();
        public List<int> Demonstration { get; set; } = new List<int>();
        public List<int> Integration { get; set; } = new List<int>();

        public string Topic { get; set; } = "";
        public int Percentage1 { get; set; } = 0;
        public int Percentage2 { get; set; } = 0;
        public int Percentage3 { get; set; } = 0;
        public int Percentage4 { get; set; } = 0;
        public int Percentage5 { get; set; } = 0;
    }
}
