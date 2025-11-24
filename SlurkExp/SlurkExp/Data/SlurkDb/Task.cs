namespace SlurkExp.Data.SlurkDb;

public partial class Task
{
    public int Id { get; set; }

    public DateTime DateCreated { get; set; }

    public DateTime? DateModified { get; set; }

    public string Name { get; set; }

    public int NumUsers { get; set; }

    public int LayoutId { get; set; }

    public virtual Layout Layout { get; set; }

    public virtual ICollection<Token> Tokens { get; set; } = new List<Token>();
}
