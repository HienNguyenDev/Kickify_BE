using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Entities;
using Kickify.Infrastructure.Database;
using Kickify.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kickify.Infrastructure.Repositories;

public class NotificationPreferenceRepository : GenericRepository<NotificationPreference>, INotificationPreferenceRepository
{
    public NotificationPreferenceRepository(ApplicationDbContext context) : base(context) { }

    public async Task<NotificationPreference?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);
    }
}
