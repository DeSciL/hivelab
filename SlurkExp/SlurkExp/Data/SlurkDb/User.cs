namespace SlurkExp.Data.SlurkDb;

public partial class User
{
    public int Id { get; set; }

    public DateTime DateCreated { get; set; }

    public DateTime? DateModified { get; set; }

    public string Name { get; set; }

    public string TokenId { get; set; }

    public string SessionId { get; set; }

    public virtual ICollection<Log> LogReceivers { get; set; } = new List<Log>();

    public virtual ICollection<Log> LogUsers { get; set; } = new List<Log>();

    public virtual Token Token { get; set; }

    public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();
}
