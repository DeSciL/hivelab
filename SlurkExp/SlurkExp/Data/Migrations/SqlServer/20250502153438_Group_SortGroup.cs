using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SlurkExp.Migrations
{
    /// <inheritdoc />
    public partial class Group_SortGroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SortGroup",
                table: "Groups",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SortGroup",
                table: "Groups");
        }
    }
}
