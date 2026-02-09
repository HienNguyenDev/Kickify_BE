using Hangfire;
using Kickify.Application.Abstractions.Jobs;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace Kickify.Infrastructure.Jobs;

public class RoomAutoCloseService : IRoomAutoCloseService
{
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public RoomAutoCloseService(
        IBackgroundJobClient backgroundJobClient,
        IServiceScopeFactory serviceScopeFactory)
    {
        _backgroundJobClient = backgroundJobClient;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public void ScheduleAutoClose(Guid roomId, TimeSpan delay)
    {
        var jobId = _backgroundJobClient.Schedule(
            () => CloseRoomAsync(roomId),
            delay);

        using var scope = _serviceScopeFactory.CreateScope();
        var matchRoomRepository = scope.ServiceProvider.GetRequiredService<IMatchRoomRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var room = matchRoomRepository.GetByIdAsync(roomId).GetAwaiter().GetResult();
        if (room != null)
        {
            room.AutoCloseJobId = jobId;
            matchRoomRepository.Update(room);
            unitOfWork.SaveChangesAsync(CancellationToken.None).GetAwaiter().GetResult();
        }
    }

    public void CancelAutoClose(string? jobId)
    {
        if (string.IsNullOrEmpty(jobId)) return;
        _backgroundJobClient.Delete(jobId);
    }

    public void RescheduleAutoClose(Guid roomId, string? oldJobId, TimeSpan delay)
    {
        CancelAutoClose(oldJobId);
        ScheduleAutoClose(roomId, delay);
    }

    public async Task CloseRoomAsync(Guid roomId)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var matchRoomRepository = scope.ServiceProvider.GetRequiredService<IMatchRoomRepository>();
        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        var room = await matchRoomRepository.GetByIdAsync(roomId);
        if (room == null) return;

        if (room.Status != RoomStatus.Open) return;

        room.Status = RoomStatus.Cancelled;
        room.AutoCloseJobId = null;
        matchRoomRepository.Update(room);
        await unitOfWork.SaveChangesAsync(CancellationToken.None);
    }
}
