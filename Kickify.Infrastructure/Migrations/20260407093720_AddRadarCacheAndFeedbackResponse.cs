using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kickify.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRadarCacheAndFeedbackResponse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.CreateTable(
                name: "PlayerRadarSnapshots",
                schema: "evaluation",
                columns: table => new
                {
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Form = table.Column<decimal>(type: "numeric(6,4)", nullable: false),
                    WinRate = table.Column<decimal>(type: "numeric(6,4)", nullable: false),
                    CommunityScore = table.Column<decimal>(type: "numeric(6,4)", nullable: false),
                    Trust = table.Column<decimal>(type: "numeric(6,2)", nullable: false),
                    Contribution = table.Column<decimal>(type: "numeric(6,4)", nullable: false),
                    AssessmentsJson = table.Column<string>(type: "text", nullable: false),
                    Summary = table.Column<string>(type: "text", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerRadarSnapshots", x => x.PlayerId);
                    table.ForeignKey(
                        name: "FK_PlayerRadarSnapshots_Users_PlayerId",
                        column: x => x.PlayerId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlayerRadarSnapshots",
                schema: "evaluation");

            migrationBuilder.DropColumn(
                name: "ResponseDate",
                schema: "evaluation",
                table: "MatchFeedbacks");

            migrationBuilder.DropColumn(
                name: "RevieweeResponse",
                schema: "evaluation",
                table: "MatchFeedbacks");
        }
    }
}
