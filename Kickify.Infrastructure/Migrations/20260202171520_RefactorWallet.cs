using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kickify.Infrastructure.Migrations
{
    public partial class RefactorWallet : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. Create new Wallets table first (before dropping old tables)
            migrationBuilder.CreateTable(
                name: "Wallets",
                schema: "payment",
                columns: table => new
                {
                    WalletId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    WalletType = table.Column<string>(type: "text", nullable: false),
                    Balance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    BankAccountNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    BankName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AccountHolderName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsBankVerified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wallets", x => x.WalletId);
                    table.ForeignKey(
                        name: "FK_Wallets_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Wallets_UserId",
                schema: "payment",
                table: "Wallets",
                column: "UserId",
                unique: true);

            // 2. Migrate data from PlayerWallets to Wallets
            migrationBuilder.Sql(@"
                INSERT INTO payment.""Wallets"" (""WalletId"", ""UserId"", ""WalletType"", ""Balance"", ""BankAccountNumber"", ""BankName"", ""AccountHolderName"", ""IsBankVerified"", ""CreatedAt"", ""UpdatedAt"", ""DeletedAt"")
                SELECT ""PlayerWalletId"", ""UserId"", 'Player', ""Balance"", ""BankAccountNumber"", ""BankName"", ""AccountHolderName"", false, ""CreatedAt"", ""UpdatedAt"", ""DeletedAt""
                FROM payment.""PlayerWallets""
                ON CONFLICT (""UserId"") DO NOTHING;
            ");

            // 3. Migrate data from VenueWallets to Wallets (using Venue's OwnerId)
            migrationBuilder.Sql(@"
                INSERT INTO payment.""Wallets"" (""WalletId"", ""UserId"", ""WalletType"", ""Balance"", ""BankAccountNumber"", ""BankName"", ""AccountHolderName"", ""IsBankVerified"", ""CreatedAt"", ""UpdatedAt"", ""DeletedAt"")
                SELECT vw.""VenueWalletId"", v.""OwnerId"", 'VenueOwner', vw.""Balance"", vw.""BankAccountNumber"", vw.""BankName"", vw.""AccountHolderName"", vw.""IsBankVerified"", vw.""CreatedAt"", vw.""UpdatedAt"", vw.""DeletedAt""
                FROM payment.""VenueWallets"" vw
                JOIN venue.""Venues"" v ON vw.""VenueId"" = v.""VenueId""
                ON CONFLICT (""UserId"") DO UPDATE SET ""Balance"" = payment.""Wallets"".""Balance"" + EXCLUDED.""Balance"";
            ");

            // 4. Update PaymentRequests.WalletId to point to new Wallets
            migrationBuilder.Sql(@"
                UPDATE payment.""PaymentRequests"" pr
                SET ""WalletId"" = w.""WalletId""
                FROM payment.""Wallets"" w
                WHERE pr.""UserId"" = w.""UserId"";
            ");

            // 5. Create WalletTransactions table
            migrationBuilder.CreateTable(
                name: "WalletTransactions",
                schema: "payment",
                columns: table => new
                {
                    TransactionId = table.Column<Guid>(type: "uuid", nullable: false),
                    WalletId = table.Column<Guid>(type: "uuid", nullable: false),
                    TransactionType = table.Column<string>(type: "text", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    BalanceAfter = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TransactionCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ReferenceId = table.Column<Guid>(type: "uuid", nullable: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WalletTransactions", x => x.TransactionId);
                    table.ForeignKey(
                        name: "FK_WalletTransactions_Wallets_WalletId",
                        column: x => x.WalletId,
                        principalSchema: "payment",
                        principalTable: "Wallets",
                        principalColumn: "WalletId",
                        onDelete: ReferentialAction.Cascade);
                });

            // 6. Migrate PlayerWalletTransactions to WalletTransactions
            migrationBuilder.Sql(@"
                INSERT INTO payment.""WalletTransactions"" (""TransactionId"", ""WalletId"", ""TransactionType"", ""Amount"", ""BalanceAfter"", ""TransactionCode"", ""ReferenceId"", ""Description"", ""CreatedAt"")
                SELECT ""TransactionId"", ""PlayerWalletId"", ""TransactionType"", ""Amount"", ""BalanceAfter"", ""TransactionCode"", ""ReferenceId"", ""Description"", ""CreatedAt""
                FROM payment.""PlayerWalletTransactions"";
            ");

            // 7. Migrate VenueWalletTransactions to WalletTransactions
            migrationBuilder.Sql(@"
                INSERT INTO payment.""WalletTransactions"" (""TransactionId"", ""WalletId"", ""TransactionType"", ""Amount"", ""BalanceAfter"", ""TransactionCode"", ""ReferenceId"", ""Description"", ""CreatedAt"")
                SELECT ""TransactionId"", ""VenueWalletId"", ""TransactionType"", ""Amount"", ""BalanceAfter"", ""TransactionCode"", ""ReferenceId"", ""Description"", ""CreatedAt""
                FROM payment.""VenueWalletTransactions"";
            ");

            // 8. Create WalletWithdrawals table
            migrationBuilder.CreateTable(
                name: "WalletWithdrawals",
                schema: "payment",
                columns: table => new
                {
                    WithdrawalId = table.Column<Guid>(type: "uuid", nullable: false),
                    WalletId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false, defaultValue: "Pending"),
                    RequestDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProcessedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ProcessedByAdminId = table.Column<Guid>(type: "uuid", nullable: true),
                    AdminNotes = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WalletWithdrawals", x => x.WithdrawalId);
                    table.ForeignKey(
                        name: "FK_WalletWithdrawals_Users_ProcessedByAdminId",
                        column: x => x.ProcessedByAdminId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WalletWithdrawals_Wallets_WalletId",
                        column: x => x.WalletId,
                        principalSchema: "payment",
                        principalTable: "Wallets",
                        principalColumn: "WalletId",
                        onDelete: ReferentialAction.Cascade);
                });

            // 9. Migrate PlayerWithdrawals to WalletWithdrawals
            migrationBuilder.Sql(@"
                INSERT INTO payment.""WalletWithdrawals"" (""WithdrawalId"", ""WalletId"", ""Amount"", ""Status"", ""RequestDate"", ""ProcessedDate"", ""ProcessedByAdminId"", ""AdminNotes"")
                SELECT ""PlayerWithdrawalId"", ""PlayerWalletId"", ""Amount"", ""Status"", ""RequestDate"", ""ProcessedDate"", ""ProcessedByAdminId"", ""AdminNotes""
                FROM payment.""PlayerWithdrawals"";
            ");

            // 10. Migrate VenueWithdrawals to WalletWithdrawals
            migrationBuilder.Sql(@"
                INSERT INTO payment.""WalletWithdrawals"" (""WithdrawalId"", ""WalletId"", ""Amount"", ""Status"", ""RequestDate"", ""ProcessedDate"", ""ProcessedByAdminId"", ""AdminNotes"")
                SELECT ""VenueWithdrawalId"", ""VenueWalletId"", ""Amount"", ""Status"", ""RequestDate"", ""ProcessedDate"", ""ProcessedByAdminId"", ""AdminNotes""
                FROM payment.""VenueWithdrawals"";
            ");

            // 11. Now drop old tables (after data migration)
            migrationBuilder.DropTable(name: "PlayerWalletTransactions", schema: "payment");
            migrationBuilder.DropTable(name: "PlayerWithdrawals", schema: "payment");
            migrationBuilder.DropTable(name: "VenueWalletTransactions", schema: "payment");
            migrationBuilder.DropTable(name: "VenueWithdrawals", schema: "payment");
            migrationBuilder.DropTable(name: "PlayerWallets", schema: "payment");
            migrationBuilder.DropTable(name: "VenueWallets", schema: "payment");

            // 12. Drop old columns and indexes from PaymentRequests
            migrationBuilder.DropIndex(
                name: "IX_PaymentRequests_UserId_Status",
                schema: "payment",
                table: "PaymentRequests");

            migrationBuilder.DropColumn(
                name: "UserRole",
                schema: "payment",
                table: "PaymentRequests");

            // 13. Create new indexes
            migrationBuilder.CreateIndex(
                name: "IX_PaymentRequests_WalletId",
                schema: "payment",
                table: "PaymentRequests",
                column: "WalletId");

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransactions_CreatedAt",
                schema: "payment",
                table: "WalletTransactions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransactions_TransactionCode",
                schema: "payment",
                table: "WalletTransactions",
                column: "TransactionCode");

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransactions_WalletId",
                schema: "payment",
                table: "WalletTransactions",
                column: "WalletId");

            migrationBuilder.CreateIndex(
                name: "IX_WalletWithdrawals_ProcessedByAdminId",
                schema: "payment",
                table: "WalletWithdrawals",
                column: "ProcessedByAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_WalletWithdrawals_RequestDate",
                schema: "payment",
                table: "WalletWithdrawals",
                column: "RequestDate");

            migrationBuilder.CreateIndex(
                name: "IX_WalletWithdrawals_Status",
                schema: "payment",
                table: "WalletWithdrawals",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_WalletWithdrawals_WalletId",
                schema: "payment",
                table: "WalletWithdrawals",
                column: "WalletId");

            // 14. Add FK for PaymentRequests -> Wallets (now data is migrated)
            migrationBuilder.AddForeignKey(
                name: "FK_PaymentRequests_Wallets_WalletId",
                schema: "payment",
                table: "PaymentRequests",
                column: "WalletId",
                principalSchema: "payment",
                principalTable: "Wallets",
                principalColumn: "WalletId",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PaymentRequests_Wallets_WalletId",
                schema: "payment",
                table: "PaymentRequests");

            migrationBuilder.DropTable(name: "WalletTransactions", schema: "payment");
            migrationBuilder.DropTable(name: "WalletWithdrawals", schema: "payment");
            migrationBuilder.DropTable(name: "Wallets", schema: "payment");

            migrationBuilder.DropIndex(
                name: "IX_PaymentRequests_WalletId",
                schema: "payment",
                table: "PaymentRequests");

            migrationBuilder.AddColumn<string>(
                name: "UserRole",
                schema: "payment",
                table: "PaymentRequests",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "PlayerWallets",
                schema: "payment",
                columns: table => new
                {
                    PlayerWalletId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountHolderName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Balance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    BankAccountNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    BankName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerWallets", x => x.PlayerWalletId);
                    table.ForeignKey(
                        name: "FK_PlayerWallets_Users_UserId",
                        column: x => x.UserId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VenueWallets",
                schema: "payment",
                columns: table => new
                {
                    VenueWalletId = table.Column<Guid>(type: "uuid", nullable: false),
                    VenueId = table.Column<Guid>(type: "uuid", nullable: false),
                    AccountHolderName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Balance = table.Column<decimal>(type: "numeric(12,2)", nullable: false, defaultValue: 0m),
                    BankAccountNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    BankName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsBankVerified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VenueWallets", x => x.VenueWalletId);
                    table.ForeignKey(
                        name: "FK_VenueWallets_Venues_VenueId",
                        column: x => x.VenueId,
                        principalSchema: "venue",
                        principalTable: "Venues",
                        principalColumn: "VenueId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlayerWalletTransactions",
                schema: "payment",
                columns: table => new
                {
                    TransactionId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerWalletId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    BalanceAfter = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    ReferenceId = table.Column<Guid>(type: "uuid", nullable: true),
                    TransactionCode = table.Column<string>(type: "text", nullable: true),
                    TransactionType = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerWalletTransactions", x => x.TransactionId);
                    table.ForeignKey(
                        name: "FK_PlayerWalletTransactions_PlayerWallets_PlayerWalletId",
                        column: x => x.PlayerWalletId,
                        principalSchema: "payment",
                        principalTable: "PlayerWallets",
                        principalColumn: "PlayerWalletId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlayerWithdrawals",
                schema: "payment",
                columns: table => new
                {
                    PlayerWithdrawalId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerWalletId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcessedByAdminId = table.Column<Guid>(type: "uuid", nullable: true),
                    AdminNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ProcessedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RequestDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    Status = table.Column<string>(type: "text", nullable: false, defaultValue: "Pending")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerWithdrawals", x => x.PlayerWithdrawalId);
                    table.ForeignKey(
                        name: "FK_PlayerWithdrawals_PlayerWallets_PlayerWalletId",
                        column: x => x.PlayerWalletId,
                        principalSchema: "payment",
                        principalTable: "PlayerWallets",
                        principalColumn: "PlayerWalletId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayerWithdrawals_Users_ProcessedByAdminId",
                        column: x => x.ProcessedByAdminId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "VenueWalletTransactions",
                schema: "payment",
                columns: table => new
                {
                    TransactionId = table.Column<Guid>(type: "uuid", nullable: false),
                    VenueWalletId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    BalanceAfter = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    ReferenceId = table.Column<Guid>(type: "uuid", nullable: true, comment: "booking_id or withdrawal_id"),
                    TransactionCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    TransactionType = table.Column<string>(type: "text", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "VenueWithdrawals",
                schema: "payment",
                columns: table => new
                {
                    VenueWithdrawalId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProcessedByAdminId = table.Column<Guid>(type: "uuid", nullable: true),
                    VenueWalletId = table.Column<Guid>(type: "uuid", nullable: false),
                    AdminNotes = table.Column<string>(type: "text", nullable: true),
                    Amount = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    ProcessedDate = table.Column<DateTime>(type: "timestamp", nullable: true),
                    RequestDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false, defaultValue: "Pending")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VenueWithdrawals", x => x.VenueWithdrawalId);
                    table.ForeignKey(
                        name: "FK_VenueWithdrawals_Users_ProcessedByAdminId",
                        column: x => x.ProcessedByAdminId,
                        principalSchema: "identity",
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_VenueWithdrawals_VenueWallets_VenueWalletId",
                        column: x => x.VenueWalletId,
                        principalSchema: "payment",
                        principalTable: "VenueWallets",
                        principalColumn: "VenueWalletId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRequests_UserId_Status",
                schema: "payment",
                table: "PaymentRequests",
                columns: new[] { "UserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_PlayerWallets_UserId",
                schema: "payment",
                table: "PlayerWallets",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlayerWalletTransactions_CreatedAt",
                schema: "payment",
                table: "PlayerWalletTransactions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerWalletTransactions_PlayerWalletId",
                schema: "payment",
                table: "PlayerWalletTransactions",
                column: "PlayerWalletId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerWithdrawals_PlayerWalletId",
                schema: "payment",
                table: "PlayerWithdrawals",
                column: "PlayerWalletId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerWithdrawals_ProcessedByAdminId",
                schema: "payment",
                table: "PlayerWithdrawals",
                column: "ProcessedByAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerWithdrawals_Status",
                schema: "payment",
                table: "PlayerWithdrawals",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_VenueWallets_VenueId",
                schema: "payment",
                table: "VenueWallets",
                column: "VenueId",
                unique: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_VenueWithdrawals_ProcessedByAdminId",
                schema: "payment",
                table: "VenueWithdrawals",
                column: "ProcessedByAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_VenueWithdrawals_Status",
                schema: "payment",
                table: "VenueWithdrawals",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_VenueWithdrawals_VenueWalletId",
                schema: "payment",
                table: "VenueWithdrawals",
                column: "VenueWalletId");
        }
    }
}
