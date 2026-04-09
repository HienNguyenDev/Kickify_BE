namespace Kickify.Application.Abstractions.Jobs;

public interface ISystemLogCleanupService
{
    /// <summary>
    /// Delete old system logs based on retention policy.
    /// </summary>
    Task CleanupOldSystemLogsAsync();

    /// <summary>
    /// Schedule recurring cleanup job.
    /// </summary>
    void ScheduleSystemLogCleanup();
}
