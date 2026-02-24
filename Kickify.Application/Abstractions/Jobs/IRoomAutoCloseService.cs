namespace Kickify.Application.Abstractions.Jobs;

public interface IRoomAutoCloseService
{
    void ScheduleAutoClose(Guid roomId, TimeSpan delay);
    void CancelAutoClose(string? jobId);
    void RescheduleAutoClose(Guid roomId, string? oldJobId, TimeSpan delay);
}
