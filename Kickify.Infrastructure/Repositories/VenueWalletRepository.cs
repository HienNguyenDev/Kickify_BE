using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Entities;
using Kickify.Infrastructure.Database;
using Kickify.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kickify.Infrastructure.Repositories
{
    public class VenueWalletRepository : GenericRepository<VenueWallet>, IVenueWalletRepository
    {
        public VenueWalletRepository(ApplicationDbContext context) : base(context) { }

        public async Task<VenueWallet?> GetByVenueIdAsync(Guid venueId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.VenueId == venueId, cancellationToken);
        }

        public async Task<VenueWallet?> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(w => w.Venue)
                .AsNoTracking()
                .FirstOrDefaultAsync(w => w.Venue.OwnerId == ownerId, cancellationToken);
        }
    }
}
