using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kickify.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RenameK3WinRateToWinStreak : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "K3WinRateComponent",
                schema: "evaluation",
                table: "EloHistories",
                newName: "K3WinStreakComponent");

            migrationBuilder.RenameColumn(
                name: "K3WinRate",
                schema: "evaluation",
                table: "EloConfigurations",
                newName: "K3WinStreak");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "K3WinStreakComponent",
                schema: "evaluation",
                table: "EloHistories",
                newName: "K3WinRateComponent");

            migrationBuilder.RenameColumn(
                name: "K3WinStreak",
                schema: "evaluation",
                table: "EloConfigurations",
                newName: "K3WinRate");
        }
    }
}
