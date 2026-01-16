using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kickify.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixChatMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<Guid>(
                name: "RoomId",
                schema: "social",
                table: "ChatMessages",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<string>(
                name: "ConversationType",
                schema: "social",
                table: "ChatMessages",
                type: "text",
                nullable: false,
                defaultValue: "Private");

            migrationBuilder.AddColumn<bool>(
                name: "IsRead",
                schema: "social",
                table: "ChatMessages",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "ReceiverId",
                schema: "social",
                table: "ChatMessages",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_ReceiverId",
                schema: "social",
                table: "ChatMessages",
                column: "ReceiverId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_ReceiverId_IsRead",
                schema: "social",
                table: "ChatMessages",
                columns: new[] { "ReceiverId", "IsRead" },
                filter: "\"ReceiverId\" IS NOT NULL AND \"IsRead\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_SenderId_ReceiverId_SentAt",
                schema: "social",
                table: "ChatMessages",
                columns: new[] { "SenderId", "ReceiverId", "SentAt" });

            migrationBuilder.AddForeignKey(
                name: "FK_ChatMessages_Users_ReceiverId",
                schema: "social",
                table: "ChatMessages",
                column: "ReceiverId",
                principalSchema: "identity",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatMessages_Users_ReceiverId",
                schema: "social",
                table: "ChatMessages");

            migrationBuilder.DropIndex(
                name: "IX_ChatMessages_ReceiverId",
                schema: "social",
                table: "ChatMessages");

            migrationBuilder.DropIndex(
                name: "IX_ChatMessages_ReceiverId_IsRead",
                schema: "social",
                table: "ChatMessages");

            migrationBuilder.DropIndex(
                name: "IX_ChatMessages_SenderId_ReceiverId_SentAt",
                schema: "social",
                table: "ChatMessages");

            migrationBuilder.DropColumn(
                name: "ConversationType",
                schema: "social",
                table: "ChatMessages");

            migrationBuilder.DropColumn(
                name: "IsRead",
                schema: "social",
                table: "ChatMessages");

            migrationBuilder.DropColumn(
                name: "ReceiverId",
                schema: "social",
                table: "ChatMessages");

            migrationBuilder.AlterColumn<Guid>(
                name: "RoomId",
                schema: "social",
                table: "ChatMessages",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }
    }
}
