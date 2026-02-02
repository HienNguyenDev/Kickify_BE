using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Infrastructure.Database;
using Kickify.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kickify.Infrastructure.Repositories;

public class WalletWithdrawalRepository : GenericRepository<WalletWithdrawal>, IWalletWithdrawalRepository
{
    public WalletWithdrawalRepository(ApplicationDbContext context) : base(context) { }

    public async Task<(IEnumerable<WalletWithdrawal> Withdrawals, int Total)> GetByWalletIdAsync(
        Guid walletId,
        WithdrawalStatus? status = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet.Where(w => w.WalletId == walletId);

        if (status.HasValue)
        {
            query = query.Where(w => w.Status == status.Value);
        }

        var total = await query.CountAsync(cancellationToken);
        var withdrawals = await query
            .OrderByDescending(w => w.RequestDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (withdrawals, total);
    }

    public async Task<(IEnumerable<WalletWithdrawal> Withdrawals, int Total)> GetAllAsync(
        WithdrawalStatus? status = null,
        WalletType? walletType = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .Include(w => w.Wallet)
            .ThenInclude(w => w.User)
            .Include(w => w.ProcessedByAdmin)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(w => w.Status == status.Value);
        }

        if (walletType.HasValue)
        {
            query = query.Where(w => w.Wallet.WalletType == walletType.Value);
        }

        var total = await query.CountAsync(cancellationToken);
        var withdrawals = await query
            .OrderByDescending(w => w.RequestDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (withdrawals, total);
    }

    public async Task<bool> HasPendingWithdrawalAsync(Guid walletId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.AnyAsync(
            w => w.WalletId == walletId && 
                 (w.Status == WithdrawalStatus.Pending || w.Status == WithdrawalStatus.Processing),
            cancellationToken);
    }
}
