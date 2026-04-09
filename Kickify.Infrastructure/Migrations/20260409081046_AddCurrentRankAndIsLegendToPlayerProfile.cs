using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kickify.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCurrentRankAndIsLegendToPlayerProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CurrentRank",
                schema: "identity",
                table: "PlayerProfiles",
                type: "character varying(32)",
                maxLength: 32,
                nullable: false,
                defaultValue: "Amateur");

            migrationBuilder.AddColumn<bool>(
                name: "IsLegend",
                schema: "identity",
                table: "PlayerProfiles",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentRank",
                schema: "identity",
                table: "PlayerProfiles");

            migrationBuilder.DropColumn(
                name: "IsLegend",
                schema: "identity",
                table: "PlayerProfiles");
        }
    }
}
