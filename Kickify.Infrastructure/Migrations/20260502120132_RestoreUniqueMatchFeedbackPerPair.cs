using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kickify.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RestoreUniqueMatchFeedbackPerPair : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MatchFeedbacks_MatchId_ReviewerId_RevieweeId",
                schema: "evaluation",
                table: "MatchFeedbacks");

            migrationBuilder.CreateIndex(
                name: "IX_MatchFeedbacks_MatchId_ReviewerId_RevieweeId",
                schema: "evaluation",
                table: "MatchFeedbacks",
                columns: new[] { "MatchId", "ReviewerId", "RevieweeId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_MatchFeedbacks_MatchId_ReviewerId_RevieweeId",
                schema: "evaluation",
                table: "MatchFeedbacks");

            migrationBuilder.CreateIndex(
                name: "IX_MatchFeedbacks_MatchId_ReviewerId_RevieweeId",
                schema: "evaluation",
                table: "MatchFeedbacks",
                columns: new[] { "MatchId", "ReviewerId", "RevieweeId" });
        }
    }
}
