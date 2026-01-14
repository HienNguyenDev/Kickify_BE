using Kickify.Application.Abstractions.Persistence;
using Kickify.Domain.Entities;

namespace Kickify.Application.Abstractions.Repositories
{
    public interface IMatchRoomRepository : IGenericRepository<MatchRoom>
    {
        Task<MatchRoom?> GetRoomWithParticipantsAsync(Guid roomId, CancellationToken cancellationToken = default);
        Task<bool> AreAllParticipantsPaidAsync(Guid roomId, CancellationToken cancellationToken = default);
        Task<decimal> GetTotalPaidAmountAsync(Guid roomId, CancellationToken cancellationToken = default);
    }
}
