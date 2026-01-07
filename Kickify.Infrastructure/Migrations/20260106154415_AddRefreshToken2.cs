using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kickify.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRefreshToken2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RefreshToken_Users_UserId",
                schema: "public",
                table: "RefreshToken");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RefreshToken",
                schema: "public",
                table: "RefreshToken");

            migrationBuilder.RenameTable(
                name: "RefreshToken",
                schema: "public",
                newName: "RefreshTokens",
                newSchema: "identity");

            migrationBuilder.RenameIndex(
                name: "IX_RefreshToken_UserId",
                schema: "identity",
                table: "RefreshTokens",
                newName: "IX_RefreshTokens_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_RefreshToken_Token",
                schema: "identity",
                table: "RefreshTokens",
                newName: "IX_RefreshTokens_Token");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RefreshTokens",
                schema: "identity",
                table: "RefreshTokens",
                column: "TokenId");

            migrationBuilder.AddForeignKey(
                name: "FK_RefreshTokens_Users_UserId",
                schema: "identity",
                table: "RefreshTokens",
                column: "UserId",
                principalSchema: "identity",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RefreshTokens_Users_UserId",
                schema: "identity",
                table: "RefreshTokens");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RefreshTokens",
                schema: "identity",
                table: "RefreshTokens");

            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.RenameTable(
                name: "RefreshTokens",
                schema: "identity",
                newName: "RefreshToken",
                newSchema: "public");

            migrationBuilder.RenameIndex(
                name: "IX_RefreshTokens_UserId",
                schema: "public",
                table: "RefreshToken",
                newName: "IX_RefreshToken_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_RefreshTokens_Token",
                schema: "public",
                table: "RefreshToken",
                newName: "IX_RefreshToken_Token");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RefreshToken",
                schema: "public",
                table: "RefreshToken",
                column: "TokenId");

            migrationBuilder.AddForeignKey(
                name: "FK_RefreshToken_Users_UserId",
                schema: "public",
                table: "RefreshToken",
                column: "UserId",
                principalSchema: "identity",
                principalTable: "Users",
                principalColumn: "UserId",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
