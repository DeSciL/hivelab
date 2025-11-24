namespace SlurkExp.Data.SlurkDb;

public partial class Token
{
    public DateTime DateCreated { get; set; }

    public DateTime? DateModified { get; set; }

    public string Id { get; set; }

    public int PermissionsId { get; set; }

    public int RegistrationsLeft { get; set; }

    public int? TaskId { get; set; }

    public int? RoomId { get; set; }

    public byte[] OpenviduSettings { get; set; }

    public virtual Permission Permissions { get; set; }

    public virtual Room Room { get; set; }

    public virtual Task Task { get; set; }

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
