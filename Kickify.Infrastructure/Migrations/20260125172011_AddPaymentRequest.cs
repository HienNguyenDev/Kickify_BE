using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kickify.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPaymentRequest : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WalletTransactions",
                schema: "venue");

            migrationBuilder.EnsureSchema(
                name: "payment");

            migrationBuilder.RenameTable(
                name: "VenueWithdrawals",
                schema: "venue",
                newName: "VenueWithdrawals",
                newSchema: "payment");

            migrationBuilder.RenameTable(
                name: "VenueWallets",
                schema: "venue",
                newName: "VenueWallets",
                newSchema: "payment");

            migrationBuilder.RenameTable(
                name: "PlayerWithdrawals",
                schema: "identity",
                newName: "PlayerWithdrawals",
                newSchema: "payment");

            migrationBuilder.RenameTable(
                name: "PlayerWalletTransactions",
                schema: "identity",
                newName: "PlayerWalletTransactions",
                newSchema: "payment");

            migrationBuilder.RenameTable(
                name: "PlayerWallets",
                schema: "identity",
                newName: "PlayerWallets",
                newSchema: "payment");

            migrationBuilder.AddColumn<string>(
                name: "TransactionCode",
                schema: "payment",
                table: "PlayerWalletTransactions",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PaymentRequests",
                schema: "payment",
                columns: table => new
                {
                    PaymentRequestId = table.Column<Guid>(type: "uuid", nullable: false),
                    TxnRef = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserRole = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    WalletId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    VnpayTransactionNo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ExpiredAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentRequests", x => x.PaymentRequestId);
                    table.ForeignKey(
                        name: "FK_PaymentRequests_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VenueWalletTransactions",
                schema: "payment",
                columns: table => new
                {
                    TransactionId = table.Column<Guid>(type: "uuid", nullable: false),
                    VenueWalletId = table.Column<Guid>(type: "uuid", nullable: false),
                    TransactionType = table.Column<string>(type: "text", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    BalanceAfter = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    TransactionCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ReferenceId = table.Column<Guid>(type: "uuid", nullable: true, comment: "booking_id or withdrawal_id"),
                    Description = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VenueWalletTransactions", x => x.TransactionId);
                    table.ForeignKey(
                        name: "FK_VenueWalletTransactions_VenueWallets_VenueWalletId",
                        column: x => x.VenueWalletId,
                        principalSchema: "payment",
                        principalTable: "VenueWallets",
                        principalColumn: "VenueWalletId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRequests_Status",
                schema: "payment",
                table: "PaymentRequests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRequests_TxnRef",
                schema: "payment",
                table: "PaymentRequests",
                column: "TxnRef",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRequests_UserId",
                schema: "payment",
                table: "PaymentRequests",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRequests_UserId_Status",
                schema: "payment",
                table: "PaymentRequests",
                columns: new[] { "UserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_VenueWalletTransactions_CreatedAt",
                schema: "payment",
                table: "VenueWalletTransactions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_VenueWalletTransactions_TransactionCode",
                schema: "payment",
                table: "VenueWalletTransactions",
                column: "TransactionCode");

            migrationBuilder.CreateIndex(
                name: "IX_VenueWalletTransactions_VenueWalletId",
                schema: "payment",
                table: "VenueWalletTransactions",
                column: "VenueWalletId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PaymentRequests",
                schema: "payment");

            migrationBuilder.DropTable(
                name: "VenueWalletTransactions",
                schema: "payment");

            migrationBuilder.DropColumn(
                name: "TransactionCode",
                schema: "payment",
                table: "PlayerWalletTransactions");

            migrationBuilder.RenameTable(
                name: "VenueWithdrawals",
                schema: "payment",
                newName: "VenueWithdrawals",
                newSchema: "venue");

            migrationBuilder.RenameTable(
                name: "VenueWallets",
                schema: "payment",
                newName: "VenueWallets",
                newSchema: "venue");

            migrationBuilder.RenameTable(
                name: "PlayerWithdrawals",
                schema: "payment",
                newName: "PlayerWithdrawals",
                newSchema: "identity");

            migrationBuilder.RenameTable(
                name: "PlayerWalletTransactions",
                schema: "payment",
                newName: "PlayerWalletTransactions",
                newSchema: "identity");

            migrationBuilder.RenameTable(
                name: "PlayerWallets",
                schema: "payment",
                newName: "PlayerWallets",
                newSchema: "identity");

            migrationBuilder.CreateTable(
                name: "WalletTransactions",
                schema: "venue",
                columns: table => new
                {
                    TransactionId = table.Column<Guid>(type: "uuid", nullable: false),
                    VenueWalletId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    BalanceAfter = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    ReferenceId = table.Column<Guid>(type: "uuid", nullable: true, comment: "booking_id or withdrawal_id"),
                    TransactionType = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WalletTransactions", x => x.TransactionId);
                    table.ForeignKey(
                        name: "FK_WalletTransactions_VenueWallets_VenueWalletId",
                        column: x => x.VenueWalletId,
                        principalSchema: "venue",
                        principalTable: "VenueWallets",
                        principalColumn: "VenueWalletId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransactions_CreatedAt",
                schema: "venue",
                table: "WalletTransactions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransactions_VenueWalletId",
                schema: "venue",
                table: "WalletTransactions",
                column: "VenueWalletId");
        }
    }
}
