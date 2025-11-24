namespace SlurkExp.Data.SlurkDb;

public partial class Permission
{
    public int Id { get; set; }

    public DateTime DateCreated { get; set; }

    public DateTime? DateModified { get; set; }

    public bool Api { get; set; }

    public bool SendMessage { get; set; }

    public bool SendHtmlMessage { get; set; }

    public bool SendImage { get; set; }

    public bool SendCommand { get; set; }

    public bool SendPrivately { get; set; }

    public bool ReceiveBoundingBox { get; set; }

    public bool Broadcast { get; set; }

    public string OpenviduRole { get; set; }

    public virtual ICollection<Token> Tokens { get; set; } = new List<Token>();
}
