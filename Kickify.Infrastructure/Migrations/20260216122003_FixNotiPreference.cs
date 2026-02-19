using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kickify.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixNotiPreference : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RelatedEntityType",
                schema: "identity",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "ChatMessages",
                schema: "identity",
                table: "NotificationPreferences");

            migrationBuilder.DropColumn(
                name: "MatchInvites",
                schema: "identity",
                table: "NotificationPreferences");

            migrationBuilder.RenameColumn(
                name: "RelatedEntityId",
                schema: "identity",
                table: "Notifications",
                newName: "SenderId");

            migrationBuilder.RenameColumn(
                name: "SystemAnnouncements",
                schema: "identity",
                table: "NotificationPreferences",
                newName: "Post");

            migrationBuilder.RenameColumn(
                name: "RoomUpdates",
                schema: "identity",
                table: "NotificationPreferences",
                newName: "MatchRoom");

            migrationBuilder.RenameColumn(
                name: "MatchResults",
                schema: "identity",
                table: "NotificationPreferences",
                newName: "Friendship");

            migrationBuilder.AddColumn<string>(
                name: "DeepLink",
                schema: "identity",
                table: "Notifications",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_SenderId",
                schema: "identity",
                table: "Notifications",
                column: "SenderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Users_SenderId",
                schema: "identity",
                table: "Notifications",
                column: "SenderId",
                principalSchema: "identity",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Users_SenderId",
                schema: "identity",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_SenderId",
                schema: "identity",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "DeepLink",
                schema: "identity",
                table: "Notifications");

            migrationBuilder.RenameColumn(
                name: "SenderId",
                schema: "identity",
                table: "Notifications",
                newName: "RelatedEntityId");

            migrationBuilder.RenameColumn(
                name: "Post",
                schema: "identity",
                table: "NotificationPreferences",
                newName: "SystemAnnouncements");

            migrationBuilder.RenameColumn(
                name: "MatchRoom",
                schema: "identity",
                table: "NotificationPreferences",
                newName: "RoomUpdates");

            migrationBuilder.RenameColumn(
                name: "Friendship",
                schema: "identity",
                table: "NotificationPreferences",
                newName: "MatchResults");

            migrationBuilder.AddColumn<string>(
                name: "RelatedEntityType",
                schema: "identity",
                table: "Notifications",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "ChatMessages",
                schema: "identity",
                table: "NotificationPreferences",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<bool>(
                name: "MatchInvites",
                schema: "identity",
                table: "NotificationPreferences",
                type: "boolean",
                nullable: false,
                defaultValue: true);
        }
    }
}
