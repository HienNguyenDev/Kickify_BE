using Hangfire;
using Kickify.Application.Abstractions.Jobs;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Kickify.Infrastructure.Jobs;

public class MatchLifecycleService : IMatchLifecycleService
{
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly ILogger<MatchLifecycleService> _logger;

    private const double VoteThresholdPercentage = 0.6; // 60%
    private static readonly TimeSpan ReviewingPeriod = TimeSpan.FromHours(12);

    public MatchLifecycleService(
        IBackgroundJobClient backgroundJobClient,
        IServiceScopeFactory serviceScopeFactory,
        ILogger<MatchLifecycleService> logger)
    {
        _backgroundJobClient = backgroundJobClient;
        _serviceScopeFactory = serviceScopeFactory;
        _logger = logger;
    }

    public void ScheduleMatchStart(Guid roomId, DateTime matchStartTime)
    {
        var delay = matchStartTime - DateTime.UtcNow;
        if (delay < TimeSpan.Zero)
        {
            delay = TimeSpan.Zero;
        }

        var jobId = _backgroundJobClient.Schedule(
            () => StartMatchAsync(roomId),
            delay);

        UpdateRoomJobId(roomId, r => r.StartMatchJobId = jobId);
        _logger.LogInformation("Scheduled match start for room {RoomId} at {StartTime}, JobId: {JobId}", 
            roomId, matchStartTime, jobId);
    }

    public void ScheduleMatchEnd(Guid roomId, DateTime matchEndTime)
    {
        var delay = matchEndTime - DateTime.UtcNow;
        if (delay < TimeSpan.Zero)
        {
            delay = TimeSpan.Zero;
        }

        var jobId = _backgroundJobClient.Schedule(
            () => EndMatchAsync(roomId),
            delay);

        UpdateRoomJobId(roomId, r => r.EndMatchJobId = jobId);
        _logger.LogInformation("Scheduled match end for room {RoomId} at {EndTime}, JobId: {JobId}", 
            roomId, matchEndTime, jobId);
    }

    public void ScheduleResultFinalization(Guid roomId, DateTime finalizeTime)
    {
        var delay = finalizeTime - DateTime.UtcNow;
        if (delay < TimeSpan.Zero)
        {
            delay = TimeSpan.Zero;
        }

        var jobId = _backgroundJobClient.Schedule(
            () => FinalizeMatchResultAsync(roomId),
            delay);

        UpdateRoomJobId(roomId, r => r.FinalizeResultJobId = jobId);
        _logger.LogInformation("Scheduled result finalization for room {RoomId} at {FinalizeTime}, JobId: {JobId}", 
            roomId, finalizeTime, jobId);
    }

    public void CancelAllJobs(string? startJobId, string? endJobId, string? finalizeJobId)
    {
        if (!string.IsNullOrEmpty(startJobId))
            _backgroundJobClient.Delete(startJobId);
        if (!string.IsNullOrEmpty(endJobId))
            _backgroundJobClient.Delete(endJobId);
        if (!string.IsNullOrEmpty(finalizeJobId))
            _backgroundJobClient.Delete(finalizeJobId);
    }

    public async Task StartMatchAsync(Guid roomId)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var matchRoomRepository = scope.ServiceProvider.GetRequiredService<IMatchRoomRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var room = await matchRoomRepository.GetByIdAsync(roomId);
        if (room == null)
        {
            _logger.LogWarning("Room {RoomId} not found for match start", roomId);
            return;
        }

        if (room.Status != RoomStatus.Locked)
        {
            _logger.LogWarning("Room {RoomId} is not in Locked status, cannot start match. Current status: {Status}", 
                roomId, room.Status);
            return;
        }

        room.Status = RoomStatus.InProgress;
        room.StartMatchJobId = null;
        matchRoomRepository.Update(room);
        await unitOfWork.SaveChangesAsync(CancellationToken.None);

        _logger.LogInformation("Match started for room {RoomId}", roomId);

        // Schedule match end
        var matchEndTime = room.MatchDate.Add(room.StartTime).AddMinutes(room.DurationMinutes);
        ScheduleMatchEnd(roomId, matchEndTime);
    }

    public async Task EndMatchAsync(Guid roomId)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var matchRoomRepository = scope.ServiceProvider.GetRequiredService<IMatchRoomRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var room = await matchRoomRepository.GetByIdAsync(roomId);
        if (room == null)
        {
            _logger.LogWarning("Room {RoomId} not found for match end", roomId);
            return;
        }

        if (room.Status != RoomStatus.InProgress)
        {
            _logger.LogWarning("Room {RoomId} is not in InProgress status, cannot end match. Current status: {Status}", 
                roomId, room.Status);
            return;
        }

        room.Status = RoomStatus.Reviewing;
        room.EndMatchJobId = null;
        matchRoomRepository.Update(room);
        await unitOfWork.SaveChangesAsync(CancellationToken.None);

        _logger.LogInformation("Match ended for room {RoomId}, entering reviewing phase", roomId);

        // Schedule result finalization after 12 hours
        var finalizeTime = DateTime.UtcNow.Add(ReviewingPeriod);
        ScheduleResultFinalization(roomId, finalizeTime);
    }

    public async Task FinalizeMatchResultAsync(Guid roomId)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var matchRoomRepository = scope.ServiceProvider.GetRequiredService<IMatchRoomRepository>();
        var matchResultVoteRepository = scope.ServiceProvider.GetRequiredService<IMatchResultVoteRepository>();
        var roomParticipantRepository = scope.ServiceProvider.GetRequiredService<IRoomParticipantRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var room = await matchRoomRepository.GetByIdAsync(roomId);
        if (room == null)
        {
            _logger.LogWarning("Room {RoomId} not found for result finalization", roomId);
            return;
        }

        if (room.Status != RoomStatus.Reviewing)
        {
            _logger.LogWarning("Room {RoomId} is not in Reviewing status, cannot finalize. Current status: {Status}", 
                roomId, room.Status);
            return;
        }

        var votes = await matchResultVoteRepository.GetVotesByRoomAsync(roomId);
        var totalParticipants = room.FilledSlots;

        if (votes.Count == 0)
        {
            // No votes at all - cancel the room
            room.Status = RoomStatus.Cancelled;
            room.FinalizeResultJobId = null;
            matchRoomRepository.Update(room);
            await unitOfWork.SaveChangesAsync(CancellationToken.None);

            _logger.LogInformation("Room {RoomId} cancelled due to no votes", roomId);
            return;
        }

        // Group votes by result and find the winner
        var voteGroups = votes
            .GroupBy(v => v.Vote)
            .Select(g => new { Result = g.Key, Count = g.Count() })
            .OrderByDescending(g => g.Count)
            .ToList();

        var winningResult = voteGroups.First().Result;
        room.FinalResult = winningResult;
        room.Status = RoomStatus.Completed;
        room.ResultConfirmedBy = votes.Count;
        room.FinalizeResultJobId = null;
        matchRoomRepository.Update(room);
        await unitOfWork.SaveChangesAsync(CancellationToken.None);

        _logger.LogInformation("Room {RoomId} finalized with result {Result}. Votes: {VoteCount}/{TotalParticipants}", 
            roomId, winningResult, votes.Count, totalParticipants);
    }

    public async Task CheckAndFinalizeIfThresholdMetAsync(Guid roomId)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var matchRoomRepository = scope.ServiceProvider.GetRequiredService<IMatchRoomRepository>();
        var matchResultVoteRepository = scope.ServiceProvider.GetRequiredService<IMatchResultVoteRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var room = await matchRoomRepository.GetByIdAsync(roomId);
        if (room == null || room.Status != RoomStatus.Reviewing)
        {
            return;
        }

        var totalParticipants = room.FilledSlots;
        var voteCount = await matchResultVoteRepository.GetVoteCountByRoomAsync(roomId);
        var votePercentage = (double)voteCount / totalParticipants;

        if (votePercentage >= VoteThresholdPercentage)
        {
            _logger.LogInformation("Room {RoomId} reached {Percentage:P0} vote threshold. Finalizing early.", 
                roomId, votePercentage);

            // Cancel the scheduled finalization job
            if (!string.IsNullOrEmpty(room.FinalizeResultJobId))
            {
                _backgroundJobClient.Delete(room.FinalizeResultJobId);
            }

            // Finalize now
            await FinalizeMatchResultAsync(roomId);
        }
    }

    private void UpdateRoomJobId(Guid roomId, Action<Domain.Entities.MatchRoom> updateAction)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var matchRoomRepository = scope.ServiceProvider.GetRequiredService<IMatchRoomRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var room = matchRoomRepository.GetByIdAsync(roomId).GetAwaiter().GetResult();
        if (room != null)
        {
            updateAction(room);
            matchRoomRepository.Update(room);
            unitOfWork.SaveChangesAsync(CancellationToken.None).GetAwaiter().GetResult();
        }
    }
}
