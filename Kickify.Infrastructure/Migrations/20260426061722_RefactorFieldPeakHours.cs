using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kickify.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefactorFieldPeakHours : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPeakHourSurchargePercentage",
                schema: "venue",
                table: "Fields");

            migrationBuilder.DropColumn(
                name: "PeakDaysOfWeek",
                schema: "venue",
                table: "Fields");

            migrationBuilder.DropColumn(
                name: "PeakEndTime",
                schema: "venue",
                table: "Fields");

            migrationBuilder.DropColumn(
                name: "PeakHourSurcharge",
                schema: "venue",
                table: "Fields");

            migrationBuilder.DropColumn(
                name: "PeakStartTime",
                schema: "venue",
                table: "Fields");

            migrationBuilder.CreateTable(
                name: "FieldPeakHours",
                schema: "venue",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FieldId = table.Column<Guid>(type: "uuid", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    SurchargeAmount = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    IsPercentage = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ApplicableDays = table.Column<int[]>(type: "integer[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FieldPeakHours", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FieldPeakHours_Fields_FieldId",
                        column: x => x.FieldId,
                        principalSchema: "venue",
                        principalTable: "Fields",
                        principalColumn: "FieldId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FieldPeakHours_FieldId",
                schema: "venue",
                table: "FieldPeakHours",
                column: "FieldId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FieldPeakHours",
                schema: "venue");

            migrationBuilder.AddColumn<bool>(
                name: "IsPeakHourSurchargePercentage",
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

            migrationBuilder.AddColumn<TimeSpan>(
                name: "PeakEndTime",
                schema: "venue",
                table: "Fields",
                type: "time",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PeakHourSurcharge",
                schema: "venue",
                table: "Fields",
                type: "numeric(10,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "PeakStartTime",
                schema: "venue",
                table: "Fields",
                type: "time",
                nullable: true);
        }
    }
}
