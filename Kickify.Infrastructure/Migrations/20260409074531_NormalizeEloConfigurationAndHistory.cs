using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kickify.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class NormalizeEloConfigurationAndHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "WinLossComponent",
                schema: "evaluation",
                table: "EloHistories",
                newName: "K1MatchResultComponent");

            migrationBuilder.RenameColumn(
                name: "FeedbackComponent",
                schema: "evaluation",
                table: "EloHistories",
                newName: "K2FeedbackSentimentComponent");

            migrationBuilder.RenameColumn(
                name: "PerformanceComponent",
                schema: "evaluation",
                table: "EloHistories",
                newName: "K3WinRateComponent");

            migrationBuilder.RenameColumn(
                name: "RoleComponent",
                schema: "evaluation",
                table: "EloHistories",
                newName: "K4ContributionComponent");

            migrationBuilder.RenameColumn(
                name: "TrustComponent",
                schema: "evaluation",
                table: "EloHistories",
                newName: "K5TrustComponent");

            migrationBuilder.RenameColumn(
                name: "KWinloss",
                schema: "evaluation",
                table: "EloConfigurations",
                newName: "K1MatchResult");

            migrationBuilder.RenameColumn(
                name: "KFeedback",
                schema: "evaluation",
                table: "EloConfigurations",
                newName: "K2FeedbackSentiment");

            migrationBuilder.RenameColumn(
                name: "KPerformance",
                schema: "evaluation",
                table: "EloConfigurations",
                newName: "K3WinRate");

            migrationBuilder.RenameColumn(
                name: "KRole",
                schema: "evaluation",
                table: "EloConfigurations",
                newName: "K4Contribution");

            migrationBuilder.DropColumn(
                name: "SentimentComponent",
                schema: "evaluation",
                table: "EloHistories");

            migrationBuilder.RenameColumn(
                name: "KTrust",
                schema: "evaluation",
                table: "EloConfigurations",
                newName: "K5Trust");

            migrationBuilder.DropColumn(
                name: "KSentiment",
                schema: "evaluation",
                table: "EloConfigurations");

            migrationBuilder.InsertData(
                schema: "evaluation",
                table: "EloConfigurations",
                columns: new[]
                {
                    "ConfigId",
                    "VersionName",
                    "K1MatchResult",
                    "K2FeedbackSentiment",
                    "K3WinRate",
                    "K4Contribution",
                    "K5Trust",
                    "EffectiveFrom",
                    "EffectiveTo",
                    "IsActive",
                    "CreatedBy",
                    "CreatedAt",
                    "UpdatedAt",
                    "DeletedAt"
                },
                values: new object[]
                {
                    new Guid("b7e4fd36-08df-45ac-9499-44adf3ffb577"),
                    "2026-04-elo-radar-v1",
                    35m,
                    10m,
                    3m,
                    1m,
                    1m,
                    new DateTime(2026, 4, 9, 0, 0, 0, DateTimeKind.Utc),
                    null,
                    true,
                    null,
                    new DateTime(2026, 4, 9, 0, 0, 0, DateTimeKind.Utc),
                    new DateTime(2026, 4, 9, 0, 0, 0, DateTimeKind.Utc),
                    null
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "K1MatchResultComponent",
                schema: "evaluation",
                table: "EloHistories",
                newName: "WinLossComponent");

            migrationBuilder.RenameColumn(
                name: "K2FeedbackSentimentComponent",
                schema: "evaluation",
                table: "EloHistories",
                newName: "FeedbackComponent");

            migrationBuilder.RenameColumn(
                name: "K3WinRateComponent",
                schema: "evaluation",
                table: "EloHistories",
                newName: "PerformanceComponent");

            migrationBuilder.RenameColumn(
                name: "K4ContributionComponent",
                schema: "evaluation",
                table: "EloHistories",
                newName: "RoleComponent");

            migrationBuilder.RenameColumn(
                name: "K5TrustComponent",
                schema: "evaluation",
                table: "EloHistories",
                newName: "TrustComponent");

            migrationBuilder.RenameColumn(
                name: "K1MatchResult",
                schema: "evaluation",
                table: "EloConfigurations",
                newName: "KWinloss");

            migrationBuilder.RenameColumn(
                name: "K2FeedbackSentiment",
                schema: "evaluation",
                table: "EloConfigurations",
                newName: "KFeedback");

            migrationBuilder.RenameColumn(
                name: "K3WinRate",
                schema: "evaluation",
                table: "EloConfigurations",
                newName: "KPerformance");

            migrationBuilder.RenameColumn(
                name: "K4Contribution",
                schema: "evaluation",
                table: "EloConfigurations",
                newName: "KRole");

            migrationBuilder.RenameColumn(
                name: "K5Trust",
                schema: "evaluation",
                table: "EloConfigurations",
                newName: "KTrust");

            migrationBuilder.AddColumn<decimal>(
                name: "SentimentComponent",
                schema: "evaluation",
                table: "EloHistories",
                type: "numeric(6,2)",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "KSentiment",
                schema: "evaluation",
                table: "EloConfigurations",
                type: "numeric(5,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.DeleteData(
                schema: "evaluation",
                table: "EloConfigurations",
                keyColumn: "ConfigId",
                keyValue: new Guid("b7e4fd36-08df-45ac-9499-44adf3ffb577"));
        }
    }
}
