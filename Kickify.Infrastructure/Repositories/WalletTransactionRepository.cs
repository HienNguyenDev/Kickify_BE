using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Infrastructure.Database;
using Kickify.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kickify.Infrastructure.Repositories;

public class WalletTransactionRepository : GenericRepository<WalletTransaction>, IWalletTransactionRepository
{
    public WalletTransactionRepository(ApplicationDbContext context) : base(context) { }

    public async Task<bool> ExistsByTransactionCodeAsync(string transactionCode, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(t => t.TransactionCode == transactionCode, cancellationToken);
    }

    public async Task<(IEnumerable<WalletTransaction> Transactions, int Total)> GetByWalletIdAsync(
        Guid walletId,
        TransactionType? transactionType = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(t => t.WalletId == walletId);

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

    public async Task<(IEnumerable<WalletTransaction> Transactions, int Total)> GetAllAsync(
        WalletType? walletType = null,
        TransactionType? transactionType = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Include(t => t.Wallet)
            .ThenInclude(w => w.User)
            .AsQueryable();

        if (walletType.HasValue)
        {
            query = query.Where(t => t.Wallet.WalletType == walletType.Value);
        }

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
