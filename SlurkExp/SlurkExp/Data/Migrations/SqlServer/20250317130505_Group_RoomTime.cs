using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SlurkExp.Migrations
{
    /// <inheritdoc />
    public partial class Group_RoomTime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ChatRoomTime",
                table: "Groups",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WaitingRoomTime",
                table: "Groups",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ChatRoomTime",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "WaitingRoomTime",
                table: "Groups");
        }
    }
}
