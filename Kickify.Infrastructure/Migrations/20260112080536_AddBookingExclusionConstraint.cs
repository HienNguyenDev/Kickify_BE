using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kickify.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBookingExclusionConstraint : Migration
    {
        /// <inheritdoc />
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Enable btree_gist extension for PostgreSQL (required for exclusion constraints)
            migrationBuilder.Sql("CREATE EXTENSION IF NOT EXISTS btree_gist;");

            // Add exclusion constraint to prevent double booking
            // This ensures that no two bookings can have the same field_id and booking_date
            // with overlapping time ranges (start_time, end_time)
            migrationBuilder.Sql(@"
                ALTER TABLE venue.""Bookings""
                ADD CONSTRAINT ""no_overlap_booking""
                EXCLUDE USING GIST (
                    ""FieldId"" WITH =,
                    ""BookingDate"" WITH =,
                    tsrange(
                        (""BookingDate"" + ""StartTime"")::timestamp,
                        (""BookingDate"" + ""EndTime"")::timestamp
                    ) WITH &&
                );
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop the exclusion constraint
            migrationBuilder.Sql(@"
                ALTER TABLE venue.""Bookings""
                DROP CONSTRAINT IF EXISTS ""no_overlap_booking"";
            ");

            // Note: We don't drop the btree_gist extension as it might be used by other tables
        }

    }
}
