using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Entities;
using Kickify.Infrastructure.Database;
using Kickify.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kickify.Infrastructure.Repositories
{
    public class RefreshTokenRepository : GenericRepository<RefreshToken>, IRefreshTokenRepository
    {
        public RefreshTokenRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<List<RefreshToken>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Where(t => t.UserId == userId)
                .ToListAsync(cancellationToken);
        }

        public void RemoveRange(List<RefreshToken> tokens)
        {
            _dbSet.RemoveRange(tokens);
        }

        public async Task<RefreshToken?> GetByTokenWithUserAsync(string token, CancellationToken cancellationToken = default)
        {
            return await _dbSet
                .Include(r => r.User)
                .FirstOrDefaultAsync(t => t.Token == token, cancellationToken);
        }

        public async Task RevokeAllUserTokensAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var activeTokens = await _dbSet
                .Where(t => t.UserId == userId && t.RevokedAt == null)
                .ToListAsync(cancellationToken);

            var revokedAt = DateTime.UtcNow;

            foreach (var token in activeTokens)
            {
                token.RevokedAt = revokedAt;
            }
        }
    }
}
