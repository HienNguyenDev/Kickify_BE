using Kickify.Application.Abstractions.Persistence;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;

namespace Kickify.Application.Abstractions.Repositories;

public interface IWalletTransactionRepository : IGenericRepository<WalletTransaction>
{
    Task<bool> ExistsByTransactionCodeAsync(string transactionCode, CancellationToken cancellationToken = default);
    Task<(IEnumerable<WalletTransaction> Transactions, int Total)> GetByWalletIdAsync(
        Guid walletId,
        TransactionType? transactionType = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);
    Task<(IEnumerable<WalletTransaction> Transactions, int Total)> GetAllAsync(
        WalletType? walletType = null,
        TransactionType? transactionType = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);
}
