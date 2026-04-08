namespace Kickify.Application.Abstractions.Jobs;

public interface IMatchLifecycleService
{
    void ScheduleMatchStart(Guid roomId, DateTime matchStartTime);
    void ScheduleMatchEnd(Guid roomId, DateTime matchEndTime);
    void ScheduleReviewingPeriodEnd(Guid roomId, DateTime closeTime);
    void SchedulePostMatchProcessing(Guid roomId, DateTime processTime);
    /// <summary>FCM: 60 phút và 30 phút trước giờ bóng lăn (hủy nếu phòng đã hủy / trận đã chạy).</summary>
    void SchedulePreMatchReminders(Guid roomId, DateTime matchStartTime);
    void CancelAllJobs(string? startJobId, string? endJobId, string? finalizeJobId);
    Task StartMatchAsync(Guid roomId);
    Task EndMatchAsync(Guid roomId);
    Task CloseReviewingPeriodAsync(Guid roomId);
    /// <summary>
    /// When every participant has submitted a match-result vote, finalizes reviewing (Completed) and cancels the scheduled 22h finalize job.
    /// </summary>
    Task TryFinalizeReviewingWhenAllVotesAsync(Guid roomId);
    Task ProcessPostMatchAsync(Guid roomId);
    Task PreMatchReminderAsync(Guid roomId, int minutesBefore);
    Task PostMatchVoteFeedbackReminderAsync(Guid roomId, int attempt);
}
