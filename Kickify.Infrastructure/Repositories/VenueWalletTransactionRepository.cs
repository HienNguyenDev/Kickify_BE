using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
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

    public async Task<(IEnumerable<VenueWalletTransaction> Transactions, int Total)> GetByWalletIdAsync(
        Guid walletId,
        TransactionType? transactionType = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(t => t.VenueWalletId == walletId);

        if (transactionType.HasValue)
        {
            query = query.Where(t => t.TransactionType == transactionType.Value);
        }

        var total = await query.CountAsync(cancellationToken);
        var transactions = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (transactions, total);
    }

    public async Task<(IEnumerable<VenueWalletTransaction> Transactions, int Total)> GetAllAsync(
        TransactionType? transactionType = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Include(t => t.VenueWallet).ThenInclude(w => w.Venue)
            .AsQueryable();

        if (transactionType.HasValue)
        {
            query = query.Where(t => t.TransactionType == transactionType.Value);
        }

        var total = await query.CountAsync(cancellationToken);
        var transactions = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (transactions, total);
    }
}
