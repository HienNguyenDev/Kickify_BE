using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kickify.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveAfkVoting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AfkVotes",
                schema: "match");

            migrationBuilder.DropColumn(
                name: "AfkVoteCount",
                schema: "match",
                table: "RoomParticipants");

            migrationBuilder.DropColumn(
                name: "IsConfirmedAfk",
                schema: "match",
                table: "RoomParticipants");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AfkVoteCount",
                schema: "match",
                table: "RoomParticipants",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsConfirmedAfk",
                schema: "match",
                table: "RoomParticipants",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "AfkVotes",
                schema: "match",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MatchRoomId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetPlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    VoterId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Team = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AfkVotes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AfkVotes_MatchRooms_MatchRoomId",
                        column: x => x.MatchRoomId,
                        principalSchema: "match",
                        principalTable: "MatchRooms",
                        principalColumn: "RoomId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AfkVotes_Users_TargetPlayerId",
                        column: x => x.TargetPlayerId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AfkVotes_Users_VoterId",
                        column: x => x.VoterId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AfkVotes_MatchRoomId",
                schema: "match",
                table: "AfkVotes",
                column: "MatchRoomId");

            migrationBuilder.CreateIndex(
                name: "IX_AfkVotes_MatchRoomId_VoterId_TargetPlayerId",
                schema: "match",
                table: "AfkVotes",
                columns: new[] { "MatchRoomId", "VoterId", "TargetPlayerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AfkVotes_TargetPlayerId",
                schema: "match",
                table: "AfkVotes",
                column: "TargetPlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_AfkVotes_VoterId",
                schema: "match",
                table: "AfkVotes",
                column: "VoterId");
        }
    }
}
