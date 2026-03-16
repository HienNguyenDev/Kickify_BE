using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kickify.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixAnnouncement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Announcements_Users_CreatorUserId",
                schema: "system",
                table: "Announcements");

            migrationBuilder.DropIndex(
                name: "IX_Announcements_CreatorUserId",
                schema: "system",
                table: "Announcements");

            migrationBuilder.DropColumn(
                name: "CreatorUserId",
                schema: "system",
                table: "Announcements");

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                schema: "system",
                table: "Announcements",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Announcements_CreatedBy",
                schema: "system",
                table: "Announcements",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Announcements_UserId",
                schema: "system",
                table: "Announcements",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Announcements_Users_CreatedBy",
                schema: "system",
                table: "Announcements",
                column: "CreatedBy",
                principalSchema: "identity",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Announcements_Users_UserId",
                schema: "system",
                table: "Announcements",
                column: "UserId",
                principalSchema: "identity",
                principalTable: "Users",
                principalColumn: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Announcements_Users_CreatedBy",
                schema: "system",
                table: "Announcements");

            migrationBuilder.DropForeignKey(
                name: "FK_Announcements_Users_UserId",
                schema: "system",
                table: "Announcements");

            migrationBuilder.DropIndex(
                name: "IX_Announcements_CreatedBy",
                schema: "system",
                table: "Announcements");

            migrationBuilder.DropIndex(
                name: "IX_Announcements_UserId",
                schema: "system",
                table: "Announcements");

            migrationBuilder.DropColumn(
                name: "UserId",
                schema: "system",
                table: "Announcements");

            migrationBuilder.AddColumn<Guid>(
                name: "CreatorUserId",
                schema: "system",
                table: "Announcements",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Announcements_CreatorUserId",
                schema: "system",
                table: "Announcements",
                column: "CreatorUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Announcements_Users_CreatorUserId",
                schema: "system",
                table: "Announcements",
                column: "CreatorUserId",
                principalSchema: "identity",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
