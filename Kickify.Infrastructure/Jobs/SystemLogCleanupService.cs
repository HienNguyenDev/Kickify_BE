using Hangfire;
using Kickify.Application.Abstractions.Jobs;
using Kickify.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Kickify.Infrastructure.Jobs;

public class SystemLogCleanupService(
    IRecurringJobManager recurringJobManager,
    ApplicationDbContext dbContext,
    ILogger<SystemLogCleanupService> logger) : ISystemLogCleanupService
{
    // Keep logs for 6 months.
    private const int RetentionMonths = 6;

    public void ScheduleSystemLogCleanup()
    {
        // Run every 3 months on day 1 at 00:00 UTC (Jan/Apr/Jul/Oct).
        recurringJobManager.AddOrUpdate(
            "cleanup-system-logs",
            () => CleanupOldSystemLogsAsync(),
            "0 0 1 1,4,7,10 *",
            TimeZoneInfo.Utc);

        logger.LogInformation("Scheduled system log cleanup job to run every 3 months.");
    }

    public async Task CleanupOldSystemLogsAsync()
    {
        try
        {
            var cutoff = DateTime.UtcNow.AddMonths(-RetentionMonths);
            var deleted = await dbContext.SystemLogs
                .Where(x => x.CreatedAt < cutoff)
                .ExecuteDeleteAsync();

            logger.LogInformation("System log cleanup completed. Deleted {DeletedCount} logs older than {Cutoff}.", deleted, cutoff);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "System log cleanup failed.");
        }
    }
}
