using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kickify.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTeamNames : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TeamAName",
                schema: "match",
                table: "MatchRooms",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TeamBName",
                schema: "match",
                table: "MatchRooms",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TeamAName",
                schema: "match",
                table: "MatchRooms");

            migrationBuilder.DropColumn(
                name: "TeamBName",
                schema: "match",
                table: "MatchRooms");
        }
    }
}
