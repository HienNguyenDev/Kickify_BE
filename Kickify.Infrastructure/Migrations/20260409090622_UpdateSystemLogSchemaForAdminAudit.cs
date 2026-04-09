using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kickify.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSystemLogSchemaForAdminAudit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IpAddress",
                schema: "system",
                table: "SystemLogs");

            migrationBuilder.DropColumn(
                name: "RequestDetails",
                schema: "system",
                table: "SystemLogs");

            migrationBuilder.AlterColumn<string>(
                name: "UserAgent",
                schema: "system",
                table: "SystemLogs",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ResponseStatus",
                schema: "system",
                table: "SystemLogs",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Error",
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.Sql("""
                UPDATE system."SystemLogs"
                SET "ResponseStatus" = CASE
                    WHEN "ResponseStatus" ~ '^[0-9]+$' AND "ResponseStatus"::int BETWEEN 200 AND 299 THEN 'Success'
                    WHEN "ResponseStatus" ~ '^[0-9]+$' AND "ResponseStatus"::int BETWEEN 400 AND 499 THEN 'Error'
                    WHEN "ResponseStatus" ~ '^[0-9]+$' AND "ResponseStatus"::int BETWEEN 500 AND 599 THEN 'ServerFailure'
                    WHEN lower("ResponseStatus") = 'success' THEN 'Success'
                    WHEN lower("ResponseStatus") = 'error' THEN 'Error'
                    WHEN lower("ResponseStatus") = 'serverfailure' THEN 'ServerFailure'
                    ELSE 'Error'
                END;
                """);

            migrationBuilder.AlterColumn<string>(
                name: "Action",
                schema: "system",
                table: "SystemLogs",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.Sql("""
                UPDATE system."SystemLogs"
                SET "Action" = CASE
                    WHEN lower("Action") = 'create' THEN 'Create'
                    WHEN lower("Action") = 'update' THEN 'Update'
                    WHEN lower("Action") = 'delete' THEN 'Delete'
                    ELSE 'Update'
                END;
                """);

            migrationBuilder.AddColumn<string>(
                name: "UserName",
                schema: "system",
                table: "SystemLogs",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserName",
                schema: "system",
                table: "SystemLogs");

            migrationBuilder.AlterColumn<string>(
                name: "UserAgent",
                schema: "system",
                table: "SystemLogs",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "ResponseStatus",
                schema: "system",
                table: "SystemLogs",
                type: "integer",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "Action",
                schema: "system",
                table: "SystemLogs",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AddColumn<string>(
                name: "IpAddress",
                schema: "system",
                table: "SystemLogs",
                type: "character varying(45)",
                maxLength: 45,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequestDetails",
                schema: "system",
                table: "SystemLogs",
                type: "text",
                nullable: true,
                comment: "JSON");
        }
    }
}
