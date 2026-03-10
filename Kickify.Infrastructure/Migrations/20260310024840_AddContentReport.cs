using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kickify.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddContentReport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ContentReports",
                schema: "social",
                columns: table => new
                {
                    ReportId = table.Column<Guid>(type: "uuid", nullable: false),
                    ReporterId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContentType = table.Column<string>(type: "text", nullable: false),
                    ContentId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContentOwnerId = table.Column<Guid>(type: "uuid", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false, defaultValue: "Pending"),
                    AdminNotes = table.Column<string>(type: "text", nullable: true),
                    ResolvedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    ResolvedAt = table.Column<DateTime>(type: "timestamp", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContentReports", x => x.ReportId);
                    table.ForeignKey(
                        name: "FK_ContentReports_Users_ContentOwnerId",
                        column: x => x.ContentOwnerId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContentReports_Users_ReporterId",
                        column: x => x.ReporterId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContentReports_Users_ResolvedBy",
                        column: x => x.ResolvedBy,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ContentReports_ContentId",
                schema: "social",
                table: "ContentReports",
                column: "ContentId");

            migrationBuilder.CreateIndex(
                name: "IX_ContentReports_ContentOwnerId",
                schema: "social",
                table: "ContentReports",
                column: "ContentOwnerId");

            migrationBuilder.CreateIndex(
                name: "IX_ContentReports_ReporterId_ContentId",
                schema: "social",
                table: "ContentReports",
                columns: new[] { "ReporterId", "ContentId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ContentReports_ResolvedBy",
                schema: "social",
                table: "ContentReports",
                column: "ResolvedBy");

            migrationBuilder.CreateIndex(
                name: "IX_ContentReports_Status",
                schema: "social",
                table: "ContentReports",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ContentReports",
                schema: "social");
        }
    }
}
