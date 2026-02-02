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
}
