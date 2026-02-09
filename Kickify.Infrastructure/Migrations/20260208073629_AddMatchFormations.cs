using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kickify.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMatchFormations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MatchFormations",
                schema: "match",
                columns: table => new
                {
                    FormationId = table.Column<Guid>(type: "uuid", nullable: false),
                    RoomId = table.Column<Guid>(type: "uuid", nullable: false),
                    TeamAssignment = table.Column<string>(type: "text", nullable: false),
                    FormationName = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    MatchFormat = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchFormations", x => x.FormationId);
                    table.ForeignKey(
                        name: "FK_MatchFormations_MatchRooms_RoomId",
                        column: x => x.RoomId,
                        principalSchema: "match",
                        principalTable: "MatchRooms",
                        principalColumn: "RoomId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "FormationAssignments",
                schema: "match",
                columns: table => new
                {
                    AssignmentId = table.Column<Guid>(type: "uuid", nullable: false),
                    FormationId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    SlotId = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FormationAssignments", x => x.AssignmentId);
                    table.ForeignKey(
                        name: "FK_FormationAssignments_MatchFormations_FormationId",
                        column: x => x.FormationId,
                        principalSchema: "match",
                        principalTable: "MatchFormations",
                        principalColumn: "FormationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_FormationAssignments_Users_PlayerId",
                        column: x => x.PlayerId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_FormationAssignments_FormationId_PlayerId",
                schema: "match",
                table: "FormationAssignments",
                columns: new[] { "FormationId", "PlayerId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FormationAssignments_FormationId_SlotId",
                schema: "match",
                table: "FormationAssignments",
                columns: new[] { "FormationId", "SlotId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FormationAssignments_PlayerId",
                schema: "match",
                table: "FormationAssignments",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchFormations_RoomId_TeamAssignment",
                schema: "match",
                table: "MatchFormations",
                columns: new[] { "RoomId", "TeamAssignment" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FormationAssignments",
                schema: "match");

            migrationBuilder.DropTable(
                name: "MatchFormations",
                schema: "match");
        }
    }
}
