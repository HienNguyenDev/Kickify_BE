using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kickify.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RestoreFieldAndAddStartTimeRulesToMatchPreset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "FieldId",
                schema: "match",
                table: "MatchPresets",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Rules",
                schema: "match",
                table: "MatchPresets",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "StartTime",
                schema: "match",
                table: "MatchPresets",
                type: "time",
                nullable: false,
                defaultValue: new TimeSpan(0, 0, 0, 0, 0));

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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
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
                name: "FieldId",
                schema: "match",
                table: "MatchPresets");

            migrationBuilder.DropColumn(
                name: "Rules",
                schema: "match",
                table: "MatchPresets");

            migrationBuilder.DropColumn(
                name: "StartTime",
                schema: "match",
                table: "MatchPresets");
        }
    }
}
