using Microsoft.EntityFrameworkCore;

namespace SlurkExp.Data.SlurkDb;

public partial class SlurkDbContext : DbContext
{
    protected readonly IConfiguration Configuration;

    public SlurkDbContext(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public virtual DbSet<Layout> Layouts { get; set; }

    public virtual DbSet<Log> Logs { get; set; }

    public virtual DbSet<Permission> Permissions { get; set; }

    public virtual DbSet<Room> Rooms { get; set; }

    public virtual DbSet<Session> Sessions { get; set; }

    public virtual DbSet<Task> Tasks { get; set; }

    public virtual DbSet<Token> Tokens { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseNpgsql(Configuration.GetConnectionString("SlurkConnection"));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Layout>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Layout_pkey");

            entity.ToTable("Layout");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Css)
                .HasColumnType("character varying")
                .HasColumnName("css");
            entity.Property(e => e.DateCreated)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("date_created");
            entity.Property(e => e.DateModified)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("date_modified");
            entity.Property(e => e.Html)
                .HasColumnType("character varying")
                .HasColumnName("html");
            entity.Property(e => e.OpenviduSettings)
                .IsRequired()
                .HasColumnName("openvidu_settings");
            entity.Property(e => e.ReadOnly).HasColumnName("read_only");
            entity.Property(e => e.Script)
                .HasColumnType("character varying")
                .HasColumnName("script");
            entity.Property(e => e.ShowLatency).HasColumnName("show_latency");
            entity.Property(e => e.ShowUsers).HasColumnName("show_users");
            entity.Property(e => e.Subtitle)
                .HasColumnType("character varying")
                .HasColumnName("subtitle");
            entity.Property(e => e.Title)
                .IsRequired()
                .HasColumnType("character varying")
                .HasColumnName("title");
        });

        modelBuilder.Entity<Log>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Log_pkey");

            entity.ToTable("Log");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Data)
                .IsRequired()
                .HasColumnType("json")
                .HasColumnName("data");
            entity.Property(e => e.DateCreated)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("date_created");
            entity.Property(e => e.DateModified)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("date_modified");
            entity.Property(e => e.Event)
                .IsRequired()
                .HasColumnType("character varying")
                .HasColumnName("event");
            entity.Property(e => e.ReceiverId).HasColumnName("receiver_id");
            entity.Property(e => e.RoomId).HasColumnName("room_id");
            entity.Property(e => e.UserId).HasColumnName("user_id");

            entity.HasOne(d => d.Receiver).WithMany(p => p.LogReceivers)
                .HasForeignKey(d => d.ReceiverId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("Log_receiver_id_fkey");

            entity.HasOne(d => d.Room).WithMany(p => p.Logs)
                .HasForeignKey(d => d.RoomId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("Log_room_id_fkey");

            entity.HasOne(d => d.User).WithMany(p => p.LogUsers)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade)
                .HasConstraintName("Log_user_id_fkey");
        });

        modelBuilder.Entity<Permission>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Permissions_pkey");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Api).HasColumnName("api");
            entity.Property(e => e.Broadcast).HasColumnName("broadcast");
            entity.Property(e => e.DateCreated)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("date_created");
            entity.Property(e => e.DateModified)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("date_modified");
            entity.Property(e => e.OpenviduRole)
                .HasColumnType("character varying")
                .HasColumnName("openvidu_role");
            entity.Property(e => e.ReceiveBoundingBox).HasColumnName("receive_bounding_box");
            entity.Property(e => e.SendCommand).HasColumnName("send_command");
            entity.Property(e => e.SendHtmlMessage).HasColumnName("send_html_message");
            entity.Property(e => e.SendImage).HasColumnName("send_image");
            entity.Property(e => e.SendMessage).HasColumnName("send_message");
            entity.Property(e => e.SendPrivately).HasColumnName("send_privately");
        });

        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Room_pkey");

            entity.ToTable("Room");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DateCreated)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("date_created");
            entity.Property(e => e.DateModified)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("date_modified");
            entity.Property(e => e.LayoutId).HasColumnName("layout_id");
            entity.Property(e => e.OpenviduSessionId)
                .HasColumnType("character varying")
                .HasColumnName("openvidu_session_id");
            entity.Property(e => e.ReadOnly).HasColumnName("read_only");

            entity.HasOne(d => d.Layout).WithMany(p => p.Rooms)
                .HasForeignKey(d => d.LayoutId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Room_layout_id_fkey");

            entity.HasOne(d => d.OpenviduSession).WithMany(p => p.Rooms)
                .HasForeignKey(d => d.OpenviduSessionId)
                .HasConstraintName("Room_openvidu_session_id_fkey");
        });

        modelBuilder.Entity<Session>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Session_pkey");

            entity.ToTable("Session");

            entity.Property(e => e.Id)
                .HasColumnType("character varying")
                .HasColumnName("id");
            entity.Property(e => e.Parameters)
                .HasColumnType("json")
                .HasColumnName("parameters");
        });

        modelBuilder.Entity<Task>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Task_pkey");

            entity.ToTable("Task");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DateCreated)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("date_created");
            entity.Property(e => e.DateModified)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("date_modified");
            entity.Property(e => e.LayoutId).HasColumnName("layout_id");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasColumnType("character varying")
                .HasColumnName("name");
            entity.Property(e => e.NumUsers).HasColumnName("num_users");

            entity.HasOne(d => d.Layout).WithMany(p => p.Tasks)
                .HasForeignKey(d => d.LayoutId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Task_layout_id_fkey");
        });

        modelBuilder.Entity<Token>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("Token_pkey");

            entity.ToTable("Token");

            entity.Property(e => e.Id)
                .HasMaxLength(36)
                .HasColumnName("id");
            entity.Property(e => e.DateCreated)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("date_created");
            entity.Property(e => e.DateModified)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("date_modified");
            entity.Property(e => e.OpenviduSettings)
                .IsRequired()
                .HasColumnName("openvidu_settings");
            entity.Property(e => e.PermissionsId).HasColumnName("permissions_id");
            entity.Property(e => e.RegistrationsLeft).HasColumnName("registrations_left");
            entity.Property(e => e.RoomId).HasColumnName("room_id");
            entity.Property(e => e.TaskId).HasColumnName("task_id");

            entity.HasOne(d => d.Permissions).WithMany(p => p.Tokens)
                .HasForeignKey(d => d.PermissionsId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("Token_permissions_id_fkey");

            entity.HasOne(d => d.Room).WithMany(p => p.Tokens)
                .HasForeignKey(d => d.RoomId)
                .HasConstraintName("Token_room_id_fkey");

            entity.HasOne(d => d.Task).WithMany(p => p.Tokens)
                .HasForeignKey(d => d.TaskId)
                .HasConstraintName("Token_task_id_fkey");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("User_pkey");

            entity.ToTable("User");

            entity.HasIndex(e => e.SessionId, "User_session_id_key").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.DateCreated)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("date_created");
            entity.Property(e => e.DateModified)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("date_modified");
            entity.Property(e => e.Name)
                .IsRequired()
                .HasColumnType("character varying")
                .HasColumnName("name");
            entity.Property(e => e.SessionId)
                .HasColumnType("character varying")
                .HasColumnName("session_id");
            entity.Property(e => e.TokenId)
                .IsRequired()
                .HasColumnType("character varying")
                .HasColumnName("token_id");

            entity.HasOne(d => d.Token).WithMany(p => p.Users)
                .HasForeignKey(d => d.TokenId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("User_token_id_fkey");

            entity.HasMany(d => d.Rooms).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "UserRoom",
                    r => r.HasOne<Room>().WithMany()
                        .HasForeignKey("RoomId")
                        .HasConstraintName("User_Room_room_id_fkey"),
                    l => l.HasOne<User>().WithMany()
                        .HasForeignKey("UserId")
                        .HasConstraintName("User_Room_user_id_fkey"),
                    j =>
                    {
                        j.HasKey("UserId", "RoomId").HasName("User_Room_pkey");
                        j.ToTable("User_Room");
                        j.IndexerProperty<int>("UserId").HasColumnName("user_id");
                        j.IndexerProperty<int>("RoomId").HasColumnName("room_id");
                    });
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
