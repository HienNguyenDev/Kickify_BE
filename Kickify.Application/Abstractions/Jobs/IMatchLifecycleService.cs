namespace Kickify.Application.Abstractions.Jobs;

public interface IMatchLifecycleService
{
    void ScheduleMatchStart(Guid roomId, DateTime matchStartTime);
    void ScheduleMatchEnd(Guid roomId, DateTime matchEndTime);
    void ScheduleReviewingPeriodEnd(Guid roomId, DateTime closeTime);
    void CancelAllJobs(string? startJobId, string? endJobId, string? finalizeJobId);
    Task StartMatchAsync(Guid roomId);
    Task EndMatchAsync(Guid roomId);
    Task CloseReviewingPeriodAsync(Guid roomId);
}
