using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Entities;
using Kickify.Infrastructure.Database;
using Kickify.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kickify.Infrastructure.Repositories;

public class VenueWalletTransactionRepository : GenericRepository<VenueWalletTransaction>, IVenueWalletTransactionRepository
{
    public VenueWalletTransactionRepository(ApplicationDbContext context) : base(context) { }

    public async Task<bool> ExistsByTransactionCodeAsync(string transactionCode, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(t => t.TransactionCode == transactionCode, cancellationToken);
    }
}
