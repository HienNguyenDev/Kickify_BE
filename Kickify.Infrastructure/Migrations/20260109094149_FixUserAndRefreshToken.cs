using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kickify.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixUserAndRefreshToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatMessages_Users_SenderId",
                schema: "social",
                table: "ChatMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Users_UserId",
                schema: "social",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_Posts_Users_UserId",
                schema: "social",
                table: "Posts");

            migrationBuilder.DropForeignKey(
                name: "FK_RoomParticipants_Users_UserId",
                schema: "match",
                table: "RoomParticipants");

            migrationBuilder.DropForeignKey(
                name: "FK_SystemLogs_Users_UserId",
                schema: "system",
                table: "SystemLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_VenueReviews_Users_UserId",
                schema: "venue",
                table: "VenueReviews");

            migrationBuilder.DropColumn(
                name: "PreferredPositions",
                schema: "identity",
                table: "PlayerProfiles");

            migrationBuilder.AddColumn<string>(
                name: "Positions",
                schema: "identity",
                table: "Users",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true,
                comment: "JSON array: [\"ST\", \"CM\", \"CB\"]");

            migrationBuilder.AddColumn<string>(
                name: "PreferredFoot",
                schema: "identity",
                table: "Users",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ShirtNumber",
                schema: "identity",
                table: "Users",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReplacedByToken",
                schema: "identity",
                table: "RefreshTokens",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RevokedAt",
                schema: "identity",
                table: "RefreshTokens",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ChatMessages_Users_SenderId",
                schema: "social",
                table: "ChatMessages",
                column: "SenderId",
                principalSchema: "identity",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Users_UserId",
                schema: "social",
                table: "Comments",
                column: "UserId",
                principalSchema: "identity",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_Users_UserId",
                schema: "social",
                table: "Posts",
                column: "UserId",
                principalSchema: "identity",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_RoomParticipants_Users_UserId",
                schema: "match",
                table: "RoomParticipants",
                column: "UserId",
                principalSchema: "identity",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_SystemLogs_Users_UserId",
                schema: "system",
                table: "SystemLogs",
                column: "UserId",
                principalSchema: "identity",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_VenueReviews_Users_UserId",
                schema: "venue",
                table: "VenueReviews",
                column: "UserId",
                principalSchema: "identity",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatMessages_Users_SenderId",
                schema: "social",
                table: "ChatMessages");

            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Users_UserId",
                schema: "social",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_Posts_Users_UserId",
                schema: "social",
                table: "Posts");

            migrationBuilder.DropForeignKey(
                name: "FK_RoomParticipants_Users_UserId",
                schema: "match",
                table: "RoomParticipants");

            migrationBuilder.DropForeignKey(
                name: "FK_SystemLogs_Users_UserId",
                schema: "system",
                table: "SystemLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_VenueReviews_Users_UserId",
                schema: "venue",
                table: "VenueReviews");

            migrationBuilder.DropColumn(
                name: "Positions",
                schema: "identity",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PreferredFoot",
                schema: "identity",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ShirtNumber",
                schema: "identity",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ReplacedByToken",
                schema: "identity",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "RevokedAt",
                schema: "identity",
                table: "RefreshTokens");

            migrationBuilder.AddColumn<string>(
                name: "PreferredPositions",
                schema: "identity",
                table: "PlayerProfiles",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true,
                comment: "JSON array: [\"ST\", \"CM\", \"CB\"]");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatMessages_Users_SenderId",
                schema: "social",
                table: "ChatMessages",
                column: "SenderId",
                principalSchema: "identity",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Users_UserId",
                schema: "social",
                table: "Comments",
                column: "UserId",
                principalSchema: "identity",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Posts_Users_UserId",
                schema: "social",
                table: "Posts",
                column: "UserId",
                principalSchema: "identity",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_RoomParticipants_Users_UserId",
                schema: "match",
                table: "RoomParticipants",
                column: "UserId",
                principalSchema: "identity",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SystemLogs_Users_UserId",
                schema: "system",
                table: "SystemLogs",
                column: "UserId",
                principalSchema: "identity",
                principalTable: "Users",
                principalColumn: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_VenueReviews_Users_UserId",
                schema: "venue",
                table: "VenueReviews",
                column: "UserId",
                principalSchema: "identity",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
