using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Entities;
using Kickify.Infrastructure.Database;
using Kickify.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kickify.Infrastructure.Repositories;

public class PlayerWalletRepository : GenericRepository<PlayerWallet>, IPlayerWalletRepository
{
    public PlayerWalletRepository(ApplicationDbContext context) : base(context) { }

    public async Task<PlayerWallet?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(w => w.UserId == userId, cancellationToken);
    }
}
