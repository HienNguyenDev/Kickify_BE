using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kickify.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentPurposeAndRoomId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Purpose",
                schema: "payment",
                table: "PaymentRequests",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "RoomId",
                schema: "payment",
                table: "PaymentRequests",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRequests_RoomId",
                schema: "payment",
                table: "PaymentRequests",
                column: "RoomId");

            migrationBuilder.AddForeignKey(
                name: "FK_PaymentRequests_MatchRooms_RoomId",
                schema: "payment",
                table: "PaymentRequests",
                column: "RoomId",
                principalSchema: "match",
                principalTable: "MatchRooms",
                principalColumn: "RoomId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentRequests_MatchRooms_RoomId",
                schema: "payment",
                table: "PaymentRequests");

            migrationBuilder.DropIndex(
                name: "IX_PaymentRequests_RoomId",
                schema: "payment",
                table: "PaymentRequests");

            migrationBuilder.DropColumn(
                name: "Purpose",
                schema: "payment",
                table: "PaymentRequests");

            migrationBuilder.DropColumn(
                name: "RoomId",
                schema: "payment",
                table: "PaymentRequests");
        }
    }
}
