using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kickify.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAchievementCriteriaAndClaimFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE evaluation."Achievements"
                SET "CriteriaType" = 'ReceivedFeedback'
                WHERE "Name" = 'Crowd Favorite';
                """);

            migrationBuilder.Sql("""
                UPDATE evaluation."Achievements"
                SET "CriteriaType" = 'Feedback'
                WHERE "Name" = 'Superstar';
                """);

            migrationBuilder.Sql("""
                DELETE FROM evaluation."PlayerAchievements"
                WHERE "AchievementId" IN (
                    SELECT "AchievementId"
                    FROM evaluation."Achievements"
                    WHERE "Name" = 'Trailblazer'
                );
                """);

            migrationBuilder.Sql("""
                DELETE FROM evaluation."Achievements"
                WHERE "Name" = 'Trailblazer';
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                INSERT INTO evaluation."Achievements"
                    ("AchievementId", "Name", "Description", "BadgeIconUrl", "CriteriaType", "CriteriaValue", "CreatedAt", "UpdatedAt", "DeletedAt")
                VALUES
                    (gen_random_uuid(), 'Trailblazer', 'Be one of the first 100 users to join Kickify.',
                     'https://via.placeholder.com/128?text=Trailblazer', 'Other', 1, now() at time zone 'utc', now() at time zone 'utc', null)
                ON CONFLICT DO NOTHING;
                """);

            migrationBuilder.Sql("""
                UPDATE evaluation."Achievements"
                SET "CriteriaType" = 'Feedback'
                WHERE "Name" = 'Crowd Favorite';
                """);
        }
    }
}
