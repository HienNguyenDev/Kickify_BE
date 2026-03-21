using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kickify.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddHolidayDynamicPricing : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "HolidaySurcharge",
                schema: "venue",
                table: "Fields",
                type: "numeric(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "PeakEndTime",
                schema: "venue",
                table: "Fields",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "PeakStartTime",
                schema: "venue",
                table: "Fields",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "WeekendSurcharge",
                schema: "venue",
                table: "Fields",
                type: "numeric(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "Holidays",
                schema: "system",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Holidays", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VenueIgnoredHolidays",
                schema: "venue",
                columns: table => new
                {
                    VenueId = table.Column<Guid>(type: "uuid", nullable: false),
                    HolidayId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VenueIgnoredHolidays", x => new { x.VenueId, x.HolidayId });
                    table.ForeignKey(
                        name: "FK_VenueIgnoredHolidays_Holidays_HolidayId",
                        column: x => x.HolidayId,
                        principalSchema: "system",
                        principalTable: "Holidays",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VenueIgnoredHolidays_Venues_VenueId",
                        column: x => x.VenueId,
                        principalSchema: "venue",
                        principalTable: "Venues",
                        principalColumn: "VenueId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Holidays_Date",
                schema: "system",
                table: "Holidays",
                column: "Date",
                unique: true);

            migrationBuilder.InsertData(
                schema: "system",
                table: "Holidays",
                columns: new[] { "Id", "Date", "Name", "CreatedAt", "UpdatedAt", "DeletedAt" },
                values: new object[,]
                {
                    { new Guid("3fa85f64-5717-4562-b3fc-2c963f66afa6"), new DateTime(2026, 4, 26, 0, 0, 0, DateTimeKind.Utc), "Giỗ tổ Hùng Vương", new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), null },
                    { new Guid("123e4567-e89b-12d3-a456-426614174000"), new DateTime(2026, 4, 30, 0, 0, 0, DateTimeKind.Utc), "Giải phóng miền Nam", new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), null },
                    { new Guid("8a6a6a16-1e5f-4cc0-a260-55bc9b6f4f11"), new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc), "Quốc tế Lao động", new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), null },
                    { new Guid("9c4e37d9-fb2b-4d38-9d4e-bf33d4d6f24a"), new DateTime(2026, 9, 2, 0, 0, 0, DateTimeKind.Utc), "Quốc khánh 2/9", new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), null },
                    { new Guid("f79ea0e7-3fb7-45f3-b3df-1b0c991690d4"), new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), "Tết Dương lịch", new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_VenueIgnoredHolidays_HolidayId",
                schema: "venue",
                table: "VenueIgnoredHolidays",
                column: "HolidayId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VenueIgnoredHolidays",
                schema: "venue");

            migrationBuilder.DropTable(
                name: "Holidays",
                schema: "system");

            migrationBuilder.DropColumn(
                name: "HolidaySurcharge",
                schema: "venue",
                table: "Fields");

            migrationBuilder.DropColumn(
                name: "PeakEndTime",
                schema: "venue",
                table: "Fields");

            migrationBuilder.DropColumn(
                name: "PeakStartTime",
                schema: "venue",
                table: "Fields");

            migrationBuilder.DropColumn(
                name: "WeekendSurcharge",
                schema: "venue",
                table: "Fields");
        }
    }
}
