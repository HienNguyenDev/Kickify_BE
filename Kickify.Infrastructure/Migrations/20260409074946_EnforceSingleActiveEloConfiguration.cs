using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kickify.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EnforceSingleActiveEloConfiguration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EloConfigurations_IsActive",
                schema: "evaluation",
                table: "EloConfigurations");

            migrationBuilder.CreateIndex(
                name: "IX_EloConfigurations_IsActive",
                schema: "evaluation",
                table: "EloConfigurations",
                column: "IsActive",
                unique: true,
                filter: "\"IsActive\" = true");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_EloConfigurations_IsActive",
                schema: "evaluation",
                table: "EloConfigurations");

            migrationBuilder.CreateIndex(
                name: "IX_EloConfigurations_IsActive",
                schema: "evaluation",
                table: "EloConfigurations",
                column: "IsActive");
        }
    }
}
