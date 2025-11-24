namespace SlurkExp.Data.SlurkDb;

public partial class Layout
{
    public int Id { get; set; }

    public DateTime DateCreated { get; set; }

    public DateTime? DateModified { get; set; }

    public string Title { get; set; }

    public string Subtitle { get; set; }

    public string Html { get; set; }

    public string Css { get; set; }

    public string Script { get; set; }

    public bool ShowUsers { get; set; }

    public bool ShowLatency { get; set; }

    public bool ReadOnly { get; set; }

    public byte[] OpenviduSettings { get; set; }

    public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();

    public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();
}
