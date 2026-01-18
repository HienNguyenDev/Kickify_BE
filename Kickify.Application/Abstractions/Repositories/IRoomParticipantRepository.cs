using Kickify.Application.Abstractions.Persistence;
using Kickify.Domain.Entities;

namespace Kickify.Application.Abstractions.Repositories
{
    public interface IRoomParticipantRepository : IGenericRepository<RoomParticipant>
    {
        Task<RoomParticipant?> GetParticipantByRoomAndUserAsync(Guid roomId, Guid userId, CancellationToken cancellationToken = default);
        Task<bool> IsUserInRoomAsync(Guid roomId, Guid userId, CancellationToken cancellationToken = default);
    }
}
