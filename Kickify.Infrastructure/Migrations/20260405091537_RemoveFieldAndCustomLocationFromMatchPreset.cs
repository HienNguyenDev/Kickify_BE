using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kickify.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RemoveFieldAndCustomLocationFromMatchPreset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MatchPresets_Fields_FieldId",
                schema: "match",
                table: "MatchPresets");

            migrationBuilder.DropIndex(
                name: "IX_MatchPresets_FieldId",
                schema: "match",
                table: "MatchPresets");

            migrationBuilder.DropColumn(
                name: "CustomLocation",
                schema: "match",
                table: "MatchPresets");

            migrationBuilder.DropColumn(
                name: "FieldId",
                schema: "match",
                table: "MatchPresets");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomLocation",
                schema: "match",
                table: "MatchPresets",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FieldId",
                schema: "match",
                table: "MatchPresets",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MatchPresets_FieldId",
                schema: "match",
                table: "MatchPresets",
                column: "FieldId");

            migrationBuilder.AddForeignKey(
                name: "FK_MatchPresets_Fields_FieldId",
                schema: "match",
                table: "MatchPresets",
                column: "FieldId",
                principalSchema: "venue",
                principalTable: "Fields",
                principalColumn: "FieldId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
