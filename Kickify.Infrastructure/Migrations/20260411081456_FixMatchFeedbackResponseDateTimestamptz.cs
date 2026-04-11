using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kickify.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixMatchFeedbackResponseDateTimestamptz : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Naive PostgreSQL "timestamp" must be interpreted as UTC when converting to timestamptz
            // so values stay aligned with DateTime.UtcNow from the application (Npgsql UTC rules).
            migrationBuilder.Sql("""
                ALTER TABLE evaluation."MatchFeedbacks"
                ALTER COLUMN "ResponseDate" TYPE timestamp with time zone
                USING (
                    CASE WHEN "ResponseDate" IS NULL THEN NULL
                    ELSE "ResponseDate" AT TIME ZONE 'UTC'
                    END
                );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                ALTER TABLE evaluation."MatchFeedbacks"
                ALTER COLUMN "ResponseDate" TYPE timestamp
                USING (
                    CASE WHEN "ResponseDate" IS NULL THEN NULL
                    ELSE ("ResponseDate" AT TIME ZONE 'UTC')
                    END
                );
                """);
        }
    }
}
