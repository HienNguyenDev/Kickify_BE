using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kickify.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVisibilityAndRoomPasswordToMatchPreset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RoomPassword",
                schema: "match",
                table: "MatchPresets",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Visibility",
                schema: "match",
                table: "MatchPresets",
                type: "text",
                nullable: false,
                defaultValue: "Public");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RoomPassword",
                schema: "match",
                table: "MatchPresets");

            migrationBuilder.DropColumn(
                name: "Visibility",
                schema: "match",
                table: "MatchPresets");
        }
    }
}
