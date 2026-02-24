using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kickify.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPlayerWalletAndWithdrawal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WalletTransactions_VenueWallets_WalletId",
                schema: "venue",
                table: "WalletTransactions");

            migrationBuilder.DropTable(
                name: "Withdrawals",
                schema: "venue");

            migrationBuilder.RenameColumn(
                name: "WalletId",
                schema: "venue",
                table: "WalletTransactions",
                newName: "VenueWalletId");

            migrationBuilder.RenameIndex(
                name: "IX_WalletTransactions_WalletId",
                schema: "venue",
                table: "WalletTransactions",
                newName: "IX_WalletTransactions_VenueWalletId");

            migrationBuilder.RenameColumn(
                name: "WalletId",
                schema: "venue",
                table: "VenueWallets",
                newName: "VenueWalletId");

            migrationBuilder.CreateTable(
                name: "PlayerWallets",
                schema: "identity",
                columns: table => new
                {
                    PlayerWalletId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Balance = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    BankAccountNumber = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    BankName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AccountHolderName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DeletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
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
                name: "VenueWithdrawals",
                schema: "venue",
                columns: table => new
                {
                    VenueWithdrawalId = table.Column<Guid>(type: "uuid", nullable: false),
                    VenueWalletId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false, defaultValue: "Pending"),
                    RequestDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProcessedDate = table.Column<DateTime>(type: "timestamp", nullable: true),
                    ProcessedByAdminId = table.Column<Guid>(type: "uuid", nullable: true),
                    AdminNotes = table.Column<string>(type: "text", nullable: true)
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
                        principalSchema: "venue",
                        principalTable: "VenueWallets",
                        principalColumn: "VenueWalletId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlayerWalletTransactions",
                schema: "identity",
                columns: table => new
                {
                    TransactionId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerWalletId = table.Column<Guid>(type: "uuid", nullable: false),
                    TransactionType = table.Column<string>(type: "text", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    BalanceAfter = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ReferenceId = table.Column<Guid>(type: "uuid", nullable: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerWalletTransactions", x => x.TransactionId);
                    table.ForeignKey(
                        name: "FK_PlayerWalletTransactions_PlayerWallets_PlayerWalletId",
                        column: x => x.PlayerWalletId,
                        principalSchema: "identity",
                        principalTable: "PlayerWallets",
                        principalColumn: "PlayerWalletId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlayerWithdrawals",
                schema: "identity",
                columns: table => new
                {
                    PlayerWithdrawalId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerWalletId = table.Column<Guid>(type: "uuid", nullable: false),
                    Amount = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false, defaultValue: "Pending"),
                    RequestDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    ProcessedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ProcessedByAdminId = table.Column<Guid>(type: "uuid", nullable: true),
                    AdminNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerWithdrawals", x => x.PlayerWithdrawalId);
                    table.ForeignKey(
                        name: "FK_PlayerWithdrawals_PlayerWallets_PlayerWalletId",
                        column: x => x.PlayerWalletId,
                        principalSchema: "identity",
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

            migrationBuilder.CreateIndex(
                name: "IX_PlayerWallets_UserId",
                schema: "identity",
                table: "PlayerWallets",
                column: "UserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlayerWalletTransactions_CreatedAt",
                schema: "identity",
                table: "PlayerWalletTransactions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerWalletTransactions_PlayerWalletId",
                schema: "identity",
                table: "PlayerWalletTransactions",
                column: "PlayerWalletId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerWithdrawals_PlayerWalletId",
                schema: "identity",
                table: "PlayerWithdrawals",
                column: "PlayerWalletId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerWithdrawals_ProcessedByAdminId",
                schema: "identity",
                table: "PlayerWithdrawals",
                column: "ProcessedByAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerWithdrawals_Status",
                schema: "identity",
                table: "PlayerWithdrawals",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_VenueWithdrawals_ProcessedByAdminId",
                schema: "venue",
                table: "VenueWithdrawals",
                column: "ProcessedByAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_VenueWithdrawals_Status",
                schema: "venue",
                table: "VenueWithdrawals",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_VenueWithdrawals_VenueWalletId",
                schema: "venue",
                table: "VenueWithdrawals",
                column: "VenueWalletId");

            migrationBuilder.AddForeignKey(
                name: "FK_WalletTransactions_VenueWallets_VenueWalletId",
                schema: "venue",
                table: "WalletTransactions",
                column: "VenueWalletId",
                principalSchema: "venue",
                principalTable: "VenueWallets",
                principalColumn: "VenueWalletId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WalletTransactions_VenueWallets_VenueWalletId",
                schema: "venue",
                table: "WalletTransactions");

            migrationBuilder.DropTable(
                name: "PlayerWalletTransactions",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "PlayerWithdrawals",
                schema: "identity");

            migrationBuilder.DropTable(
                name: "VenueWithdrawals",
                schema: "venue");

            migrationBuilder.DropTable(
                name: "PlayerWallets",
                schema: "identity");

            migrationBuilder.RenameColumn(
                name: "VenueWalletId",
                schema: "venue",
                table: "WalletTransactions",
                newName: "WalletId");

            migrationBuilder.RenameIndex(
                name: "IX_WalletTransactions_VenueWalletId",
                schema: "venue",
                table: "WalletTransactions",
                newName: "IX_WalletTransactions_WalletId");

            migrationBuilder.RenameColumn(
                name: "VenueWalletId",
                schema: "venue",
                table: "VenueWallets",
                newName: "WalletId");

            migrationBuilder.CreateTable(
                name: "Withdrawals",
                schema: "venue",
                columns: table => new
                {
                    WithdrawalId = table.Column<Guid>(type: "uuid", nullable: false),
                    WalletId = table.Column<Guid>(type: "uuid", nullable: false),
                    AdminNotes = table.Column<string>(type: "text", nullable: true),
                    Amount = table.Column<decimal>(type: "numeric(10,2)", nullable: false),
                    ProcessedDate = table.Column<DateTime>(type: "timestamp", nullable: true),
                    RequestDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false, defaultValue: "Pending")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Withdrawals", x => x.WithdrawalId);
                    table.ForeignKey(
                        name: "FK_Withdrawals_VenueWallets_WalletId",
                        column: x => x.WalletId,
                        principalSchema: "venue",
                        principalTable: "VenueWallets",
                        principalColumn: "WalletId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Withdrawals_WalletId",
                schema: "venue",
                table: "Withdrawals",
                column: "WalletId");

            migrationBuilder.AddForeignKey(
                name: "FK_WalletTransactions_VenueWallets_WalletId",
                schema: "venue",
                table: "WalletTransactions",
                column: "WalletId",
                principalSchema: "venue",
                principalTable: "VenueWallets",
                principalColumn: "WalletId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
