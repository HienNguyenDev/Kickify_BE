using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kickify.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixCommentLikeRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CommentLikes_Comments_CommentId1",
                schema: "social",
                table: "CommentLikes");

            migrationBuilder.DropIndex(
                name: "IX_CommentLikes_CommentId1",
                schema: "social",
                table: "CommentLikes");

            migrationBuilder.DropColumn(
                name: "CommentId1",
                schema: "social",
                table: "CommentLikes");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "CommentId1",
                schema: "social",
                table: "CommentLikes",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CommentLikes_CommentId1",
                schema: "social",
                table: "CommentLikes",
                column: "CommentId1");

            migrationBuilder.AddForeignKey(
                name: "FK_CommentLikes_Comments_CommentId1",
                schema: "social",
                table: "CommentLikes",
                column: "CommentId1",
                principalSchema: "social",
                principalTable: "Comments",
                principalColumn: "CommentId");
        }
    }
}
