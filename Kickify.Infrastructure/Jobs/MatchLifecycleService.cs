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

    // Thời gian cho phép vote và feedback sau trận đấu
    private static readonly TimeSpan ReviewingPeriod = TimeSpan.FromHours(22);

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

    public void ScheduleReviewingPeriodEnd(Guid roomId, DateTime closeTime)
    {
        var delay = closeTime - DateTime.UtcNow;
        if (delay < TimeSpan.Zero)
        {
            delay = TimeSpan.Zero;
        }

        var jobId = _backgroundJobClient.Schedule(
            () => CloseReviewingPeriodAsync(roomId),
            delay);

        UpdateRoomJobId(roomId, r => r.FinalizeResultJobId = jobId);
        _logger.LogInformation("Scheduled reviewing period end for room {RoomId} at {CloseTime}, JobId: {JobId}",
            roomId, closeTime, jobId);
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

        _logger.LogInformation("Match ended for room {RoomId}, entering 22-hour reviewing phase for voting and feedback", roomId);

        // Schedule đóng reviewing period sau 22 tiếng
        var closeTime = DateTime.UtcNow.Add(ReviewingPeriod);
        ScheduleReviewingPeriodEnd(roomId, closeTime);
    }

    /// <summary>
    /// Đóng giai đoạn reviewing sau 22 tiếng, không cho vote và feedback nữa
    /// </summary>
    public async Task CloseReviewingPeriodAsync(Guid roomId)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var matchRoomRepository = scope.ServiceProvider.GetRequiredService<IMatchRoomRepository>();
        var matchResultVoteRepository = scope.ServiceProvider.GetRequiredService<IMatchResultVoteRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var room = await matchRoomRepository.GetByIdAsync(roomId);
        if (room == null)
        {
            _logger.LogWarning("Room {RoomId} not found for closing reviewing period", roomId);
            return;
        }

        if (room.Status != RoomStatus.Reviewing)
        {
            _logger.LogWarning("Room {RoomId} is not in Reviewing status, cannot close. Current status: {Status}",
                roomId, room.Status);
            return;
        }

        // Lấy tất cả votes và tính kết quả cuối cùng
        var votes = await matchResultVoteRepository.GetVotesByRoomAsync(roomId);

        if (votes.Count > 0)
        {
            // Tìm kết quả có nhiều vote nhất
            var winningResult = votes
                .GroupBy(v => v.Vote)
                .OrderByDescending(g => g.Count())
                .First()
                .Key;

            room.FinalResult = winningResult;
            room.ResultConfirmedBy = votes.Count;

            _logger.LogInformation("Room {RoomId} final result determined: {Result} with {VoteCount} votes",
                roomId, winningResult, votes.Count);
        }
        else
        {
            _logger.LogInformation("Room {RoomId} completed with no votes submitted", roomId);
        }

        // Chuyển sang Completed - không cho vote và feedback nữa
        room.Status = RoomStatus.Completed;
        room.FinalizeResultJobId = null;

        matchRoomRepository.Update(room);
        await unitOfWork.SaveChangesAsync(CancellationToken.None);

        _logger.LogInformation("Room {RoomId} reviewing period closed after 22 hours. Status changed to Completed.", roomId);
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