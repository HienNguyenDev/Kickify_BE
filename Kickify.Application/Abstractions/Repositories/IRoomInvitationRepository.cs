using Kickify.Application.Abstractions.Persistence;
using Kickify.Domain.Entities;

namespace Kickify.Application.Abstractions.Repositories;

public interface IRoomInvitationRepository : IGenericRepository<RoomInvitation>
{
    Task<RoomInvitation?> GetPendingInvitationAsync(Guid roomId, Guid inviteeId, CancellationToken cancellationToken = default);
}
