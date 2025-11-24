namespace SlurkExp.Data.SlurkDb;

public partial class Session
{
    public string Id { get; set; }

    public string Parameters { get; set; }

    public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();
}
