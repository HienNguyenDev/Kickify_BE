using Kickify.Application.Abstractions.Persistence;
using Kickify.Domain.Entities;

namespace Kickify.Application.Abstractions.Repositories
{
    public interface IMatchRoomRepository : IGenericRepository<MatchRoom>
    {
        Task<MatchRoom?> GetRoomWithParticipantsAsync(Guid roomId, CancellationToken cancellationToken = default);
        Task<MatchRoom?> GetRoomWithParticipantsForUpdateAsync(Guid roomId, CancellationToken cancellationToken = default);
        Task<MatchRoom?> GetRoomWithDetailsAsync(Guid roomId, CancellationToken cancellationToken = default);
        Task<(IEnumerable<MatchRoom> Rooms, int Total)> SearchRoomsAsync(
            DateTime? date,
            string? matchFormat,
            bool? availableOnly,
            decimal? latitude,
            decimal? longitude,
            double? radiusKm,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default);
        Task<bool> AreAllParticipantsPaidAsync(Guid roomId, CancellationToken cancellationToken = default);
        Task<decimal> GetTotalPaidAmountAsync(Guid roomId, CancellationToken cancellationToken = default);
        Task<(IEnumerable<MatchRoom> Rooms, int Total)> GetMatchHistoryByUserAsync(
            Guid userId,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default);

        Task<(IEnumerable<MatchRoom> Rooms, int Total)> GetRoomsByUserAsync(
            Guid userId,
            bool? availableOnly,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default);

        Task<List<MatchRoom>> GetActiveRoomsForUserByDateAsync(Guid userId, DateTime matchDate, CancellationToken cancellationToken);

    }
}
