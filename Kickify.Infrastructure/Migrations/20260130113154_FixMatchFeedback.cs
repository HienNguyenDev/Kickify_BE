using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kickify.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixMatchFeedback : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttackRating",
                schema: "evaluation",
                table: "MatchFeedbacks");

            migrationBuilder.DropColumn(
                name: "AverageRating",
                schema: "evaluation",
                table: "MatchFeedbacks");

            migrationBuilder.DropColumn(
                name: "CommunicationRating",
                schema: "evaluation",
                table: "MatchFeedbacks");

            migrationBuilder.DropColumn(
                name: "DefenseRating",
                schema: "evaluation",
                table: "MatchFeedbacks");

            migrationBuilder.DropColumn(
                name: "FairplayRating",
                schema: "evaluation",
                table: "MatchFeedbacks");

            migrationBuilder.DropColumn(
                name: "ResponseDate",
                schema: "evaluation",
                table: "MatchFeedbacks");

            migrationBuilder.DropColumn(
                name: "RevieweeResponse",
                schema: "evaluation",
                table: "MatchFeedbacks");

            migrationBuilder.RenameColumn(
                name: "TeamworkRating",
                schema: "evaluation",
                table: "MatchFeedbacks",
                newName: "Rating");

            migrationBuilder.AlterColumn<string>(
                name: "Comment",
                schema: "evaluation",
                table: "MatchFeedbacks",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Rating",
                schema: "evaluation",
                table: "MatchFeedbacks",
                newName: "TeamworkRating");

            migrationBuilder.AlterColumn<string>(
                name: "Comment",
                schema: "evaluation",
                table: "MatchFeedbacks",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<int>(
                name: "AttackRating",
                schema: "evaluation",
                table: "MatchFeedbacks",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "1-5");

            migrationBuilder.AddColumn<decimal>(
                name: "AverageRating",
                schema: "evaluation",
                table: "MatchFeedbacks",
                type: "numeric(3,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "CommunicationRating",
                schema: "evaluation",
                table: "MatchFeedbacks",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "1-5");

            migrationBuilder.AddColumn<int>(
                name: "DefenseRating",
                schema: "evaluation",
                table: "MatchFeedbacks",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "1-5");

            migrationBuilder.AddColumn<int>(
                name: "FairplayRating",
                schema: "evaluation",
                table: "MatchFeedbacks",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                comment: "1-5");

            migrationBuilder.AddColumn<DateTime>(
                name: "ResponseDate",
                schema: "evaluation",
                table: "MatchFeedbacks",
                type: "timestamp",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RevieweeResponse",
                schema: "evaluation",
                table: "MatchFeedbacks",
                type: "text",
                nullable: true);
        }
    }
}
