using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kickify.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixPlayerProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AfkCount",
                schema: "identity",
                table: "PlayerProfiles");

            migrationBuilder.DropColumn(
                name: "PreferredPositions",
                schema: "identity",
                table: "PlayerProfiles");

            migrationBuilder.AlterColumn<int>(
                name: "TrustScore",
                schema: "identity",
                table: "PlayerProfiles",
                type: "integer",
                nullable: false,
                defaultValue: 100,
                oldClrType: typeof(decimal),
                oldType: "numeric(5,2)",
                oldDefaultValue: 100.00m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "TrustScore",
                schema: "identity",
                table: "PlayerProfiles",
                type: "numeric(5,2)",
                nullable: false,
                defaultValue: 100.00m,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 100);

            migrationBuilder.AddColumn<int>(
                name: "AfkCount",
                schema: "identity",
                table: "PlayerProfiles",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "PreferredPositions",
                schema: "identity",
                table: "PlayerProfiles",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);
        }
    }
}
