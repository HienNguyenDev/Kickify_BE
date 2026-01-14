using Kickify.Application.Abstractions.Persistence;
using Kickify.Domain.Entities;

namespace Kickify.Application.Abstractions.Repositories
{
    public interface IVenueWalletRepository : IGenericRepository<VenueWallet>
    {
        Task<VenueWallet?> GetByVenueIdAsync(Guid venueId, CancellationToken cancellationToken = default);
    }
}
