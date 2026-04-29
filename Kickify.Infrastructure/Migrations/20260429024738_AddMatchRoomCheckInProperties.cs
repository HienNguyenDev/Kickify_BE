using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kickify.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMatchRoomCheckInProperties : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CheckInLatitude",
                schema: "match",
                table: "RoomParticipants",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CheckInLongitude",
                schema: "match",
                table: "RoomParticipants",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CheckInMethod",
                schema: "match",
                table: "RoomParticipants",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CheckInPhotoUrl",
                schema: "match",
                table: "RoomParticipants",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "DistanceFromVenueMeters",
                schema: "match",
                table: "RoomParticipants",
                type: "double precision",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CheckInLatitude",
                schema: "match",
                table: "RoomParticipants");

            migrationBuilder.DropColumn(
                name: "CheckInLongitude",
                schema: "match",
                table: "RoomParticipants");

            migrationBuilder.DropColumn(
                name: "CheckInMethod",
                schema: "match",
                table: "RoomParticipants");

            migrationBuilder.DropColumn(
                name: "CheckInPhotoUrl",
                schema: "match",
                table: "RoomParticipants");

            migrationBuilder.DropColumn(
                name: "DistanceFromVenueMeters",
                schema: "match",
                table: "RoomParticipants");
        }
    }
}
