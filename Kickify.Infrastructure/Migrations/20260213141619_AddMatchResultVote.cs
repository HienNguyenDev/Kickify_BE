using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kickify.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMatchResultVote : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EndMatchJobId",
                schema: "match",
                table: "MatchRooms",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FinalResult",
                schema: "match",
                table: "MatchRooms",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FinalizeResultJobId",
                schema: "match",
                table: "MatchRooms",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "StartMatchJobId",
                schema: "match",
                table: "MatchRooms",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "match_result_votes",
                schema: "match",
                columns: table => new
                {
                    vote_id = table.Column<Guid>(type: "uuid", nullable: false),
                    room_id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    vote = table.Column<string>(type: "text", nullable: false),
                    voted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_match_result_votes", x => x.vote_id);
                    table.ForeignKey(
                        name: "FK_match_result_votes_MatchRooms_room_id",
                        column: x => x.room_id,
                        principalSchema: "match",
                        principalTable: "MatchRooms",
                        principalColumn: "RoomId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_match_result_votes_Users_user_id",
                        column: x => x.user_id,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_match_result_votes_room_id_user_id",
                schema: "match",
                table: "match_result_votes",
                columns: new[] { "room_id", "user_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_match_result_votes_user_id",
                schema: "match",
                table: "match_result_votes",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "match_result_votes",
                schema: "match");

            migrationBuilder.DropColumn(
                name: "EndMatchJobId",
                schema: "match",
                table: "MatchRooms");

            migrationBuilder.DropColumn(
                name: "FinalResult",
                schema: "match",
                table: "MatchRooms");

            migrationBuilder.DropColumn(
                name: "FinalizeResultJobId",
                schema: "match",
                table: "MatchRooms");

            migrationBuilder.DropColumn(
                name: "StartMatchJobId",
                schema: "match",
                table: "MatchRooms");
        }
    }
}
