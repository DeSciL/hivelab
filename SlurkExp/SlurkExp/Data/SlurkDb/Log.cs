namespace SlurkExp.Data.SlurkDb;

public partial class Log
{
    public int Id { get; set; }

    public DateTime DateCreated { get; set; }

    public DateTime? DateModified { get; set; }

    public string Event { get; set; }

    public int? UserId { get; set; }

    public int? RoomId { get; set; }

    public int? ReceiverId { get; set; }

    public string Data { get; set; }

    public virtual User Receiver { get; set; }

    public virtual Room Room { get; set; }

    public virtual User User { get; set; }
}
