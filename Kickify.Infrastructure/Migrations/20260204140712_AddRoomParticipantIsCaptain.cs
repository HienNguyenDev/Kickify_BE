using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kickify.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRoomParticipantIsCaptain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsBankVerified",
                schema: "payment",
                table: "Wallets");

            migrationBuilder.AddColumn<bool>(
                name: "IsCaptain",
                schema: "match",
                table: "RoomParticipants",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            // Backfill: The player with the earliest JoinDate in each Team (A or B) per Room becomes Captain
            // TeamAssignment is stored as string: "Unassigned", "A", "B"
            migrationBuilder.Sql(@"
                WITH RankedParticipants AS (
                    SELECT 
                        ""ParticipantId"",
                        ROW_NUMBER() OVER (
                            PARTITION BY ""RoomId"", ""TeamAssignment"" 
                            ORDER BY ""JoinDate"" ASC
                        ) as rn
                    FROM match.""RoomParticipants""
                    WHERE ""TeamAssignment"" IN ('A', 'B')
                )
                UPDATE match.""RoomParticipants""
                SET ""IsCaptain"" = TRUE
                FROM RankedParticipants
                WHERE match.""RoomParticipants"".""ParticipantId"" = RankedParticipants.""ParticipantId""
                AND RankedParticipants.rn = 1;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCaptain",
                schema: "match",
                table: "RoomParticipants");

            migrationBuilder.AddColumn<bool>(
                name: "IsBankVerified",
                schema: "payment",
                table: "Wallets",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
