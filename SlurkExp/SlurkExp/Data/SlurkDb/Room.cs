namespace SlurkExp.Data.SlurkDb;

public partial class Room
{
    public int Id { get; set; }

    public DateTime DateCreated { get; set; }

    public DateTime? DateModified { get; set; }

    public int LayoutId { get; set; }

    public bool ReadOnly { get; set; }

    public string OpenviduSessionId { get; set; }

    public virtual Layout Layout { get; set; }

    public virtual ICollection<Log> Logs { get; set; } = new List<Log>();

    public virtual Session OpenviduSession { get; set; }

    public virtual ICollection<Token> Tokens { get; set; } = new List<Token>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
