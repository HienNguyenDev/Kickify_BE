using Kickify.Application.Abstractions.Persistence;
using Kickify.Domain.Entities;

namespace Kickify.Application.Abstractions.Repositories;

public interface IVenueWalletTransactionRepository : IGenericRepository<VenueWalletTransaction>
{
    Task<bool> ExistsByTransactionCodeAsync(string transactionCode, CancellationToken cancellationToken = default);
}
