using Kickify.Application.Abstractions.Persistence;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;

namespace Kickify.Application.Abstractions.Repositories
{
    public interface IRoomParticipantRepository : IGenericRepository<RoomParticipant>
    {
        Task<RoomParticipant?> GetParticipantByRoomAndUserAsync(Guid roomId, Guid userId, CancellationToken cancellationToken = default);
        Task<bool> IsUserInRoomAsync(Guid roomId, Guid userId, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Check if a team in a room already has a captain
        /// </summary>
        Task<bool> HasTeamCaptainAsync(Guid roomId, TeamAssignment team, CancellationToken cancellationToken = default);
        
        /// <summary>
        /// Assign a new captain to a team when current captain leaves/switches.
        /// The player with the earliest JoinDate becomes the new captain.
        /// </summary>
        /// <returns>The new captain's UserId if found, null if team is empty</returns>
        Task<Guid?> AssignNewCaptainAsync(Guid roomId, TeamAssignment team, Guid? excludeUserId = null, CancellationToken cancellationToken = default);
    }
}
