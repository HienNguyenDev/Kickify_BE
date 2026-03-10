using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kickify.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddVenueEvidence : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                schema: "venue",
                table: "Venues",
                type: "text",
                nullable: false,
                defaultValue: "Draft",
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "Pending");

            migrationBuilder.CreateTable(
                name: "VenueEvidences",
                schema: "venue",
                columns: table => new
                {
                    EvidenceId = table.Column<Guid>(type: "uuid", nullable: false),
                    VenueId = table.Column<Guid>(type: "uuid", nullable: false),
                    FileUrl = table.Column<string>(type: "text", nullable: false),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VenueEvidences", x => x.EvidenceId);
                    table.ForeignKey(
                        name: "FK_VenueEvidences_Venues_VenueId",
                        column: x => x.VenueId,
                        principalSchema: "venue",
                        principalTable: "Venues",
                        principalColumn: "VenueId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VenueEvidences_VenueId",
                schema: "venue",
                table: "VenueEvidences",
                column: "VenueId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VenueEvidences",
                schema: "venue");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                schema: "venue",
                table: "Venues",
                type: "text",
                nullable: false,
                defaultValue: "Pending",
                oldClrType: typeof(string),
                oldType: "text",
                oldDefaultValue: "Draft");
        }
    }
}
