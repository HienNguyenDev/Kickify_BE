using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kickify.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFieldPeakDaysAndPercentageFlags : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsHolidaySurchargePercentage",
                schema: "venue",
                table: "Fields",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPeakHourSurchargePercentage",
                schema: "venue",
                table: "Fields",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsWeekendSurchargePercentage",
                schema: "venue",
                table: "Fields",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int[]>(
                name: "PeakDaysOfWeek",
                schema: "venue",
                table: "Fields",
                type: "integer[]",
                nullable: false,
                defaultValue: new int[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsHolidaySurchargePercentage",
                schema: "venue",
                table: "Fields");

            migrationBuilder.DropColumn(
                name: "IsPeakHourSurchargePercentage",
                schema: "venue",
                table: "Fields");

            migrationBuilder.DropColumn(
                name: "IsWeekendSurchargePercentage",
                schema: "venue",
                table: "Fields");

            migrationBuilder.DropColumn(
                name: "PeakDaysOfWeek",
                schema: "venue",
                table: "Fields");
        }
    }
}
