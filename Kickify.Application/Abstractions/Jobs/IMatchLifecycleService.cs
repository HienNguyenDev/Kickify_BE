namespace Kickify.Application.Abstractions.Jobs;

public interface IMatchLifecycleService
{
    /// <summary>
    /// Schedule job to start match when all players checked in
    /// </summary>
    void ScheduleMatchStart(Guid roomId, DateTime matchStartTime);
    
    /// <summary>
    /// Schedule job to end match and transition to reviewing phase
    /// </summary>
    void ScheduleMatchEnd(Guid roomId, DateTime matchEndTime);
    
    /// <summary>
    /// Schedule job to finalize match result after 12 hours of reviewing
    /// </summary>
    void ScheduleResultFinalization(Guid roomId, DateTime finalizeTime);
    
    /// <summary>
    /// Cancel all scheduled jobs for a room
    /// </summary>
    void CancelAllJobs(string? startJobId, string? endJobId, string? finalizeJobId);
    
    /// <summary>
    /// Start match - transition from Locked to InProgress
    /// </summary>
    Task StartMatchAsync(Guid roomId);
    
    /// <summary>
    /// End match - transition from InProgress to Reviewing
    /// </summary>
    Task EndMatchAsync(Guid roomId);
    
    /// <summary>
    /// Finalize match result based on votes
    /// </summary>
    Task FinalizeMatchResultAsync(Guid roomId);
    
    /// <summary>
    /// Check if 60% of players have voted and finalize early if so
    /// </summary>
    Task CheckAndFinalizeIfThresholdMetAsync(Guid roomId);
}
