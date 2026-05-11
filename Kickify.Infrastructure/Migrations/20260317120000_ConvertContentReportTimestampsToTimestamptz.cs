using Kickify.Infrastructure.Database;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kickify.Infrastructure.Migrations;

/// <summary>
/// ContentReports used PostgreSQL <c>timestamp</c> (without time zone). Npgsql 6+ rejects <see cref="System.DateTimeKind.Utc"/>
/// parameters against that type. Align with the rest of the schema (<c>timestamptz</c>).
/// </summary>
[DbContext(typeof(ApplicationDbContext))]
[Migration("20260317120000_ConvertContentReportTimestampsToTimestamptz")]
public partial class ConvertContentReportTimestampsToTimestamptz : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            ALTER TABLE social."ContentReports"
              ALTER COLUMN "CreatedAt" TYPE timestamp with time zone
              USING ("CreatedAt" AT TIME ZONE 'UTC');

            ALTER TABLE social."ContentReports"
              ALTER COLUMN "ResolvedAt" TYPE timestamp with time zone
              USING (CASE WHEN "ResolvedAt" IS NULL THEN NULL ELSE "ResolvedAt" AT TIME ZONE 'UTC' END);
            """);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql(
            """
            ALTER TABLE social."ContentReports"
              ALTER COLUMN "CreatedAt" TYPE timestamp without time zone
              USING ("CreatedAt" AT TIME ZONE 'UTC');

            ALTER TABLE social."ContentReports"
              ALTER COLUMN "ResolvedAt" TYPE timestamp without time zone
              USING (CASE WHEN "ResolvedAt" IS NULL THEN NULL ELSE "ResolvedAt" AT TIME ZONE 'UTC' END);
            """);
    }
}
