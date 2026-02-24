using Kickify.Application.Abstractions.Persistence;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;

namespace Kickify.Application.Abstractions.Repositories;

public interface IWalletWithdrawalRepository : IGenericRepository<WalletWithdrawal>
{
    Task<(IEnumerable<WalletWithdrawal> Withdrawals, int Total)> GetByWalletIdAsync(
        Guid walletId,
        WithdrawalStatus? status = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    Task<(IEnumerable<WalletWithdrawal> Withdrawals, int Total)> GetAllAsync(
        WithdrawalStatus? status = null,
        WalletType? walletType = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);

    Task<bool> HasPendingWithdrawalAsync(Guid walletId, CancellationToken cancellationToken = default);
}
