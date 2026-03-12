using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kickify.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DelVenueFeedback : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VenueFeedbacks",
                schema: "venue");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VenueFeedbacks",
                schema: "venue",
                columns: table => new
                {
                    VenueFeedbackId = table.Column<Guid>(type: "uuid", nullable: false),
                    SenderId = table.Column<Guid>(type: "uuid", nullable: false),
                    VenueId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp", nullable: false),
                    Message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Rating = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VenueFeedbacks", x => x.VenueFeedbackId);
                    table.ForeignKey(
                        name: "FK_VenueFeedbacks_Users_SenderId",
                        column: x => x.SenderId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_VenueFeedbacks_Venues_VenueId",
                        column: x => x.VenueId,
                        principalSchema: "venue",
                        principalTable: "Venues",
                        principalColumn: "VenueId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VenueFeedbacks_SenderId",
                schema: "venue",
                table: "VenueFeedbacks",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_VenueFeedbacks_VenueId",
                schema: "venue",
                table: "VenueFeedbacks",
                column: "VenueId");
        }
    }
}
