using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Entities;
using Kickify.Infrastructure.Database;
using Kickify.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kickify.Infrastructure.Repositories;

public class VenueFeedbackRepository : GenericRepository<VenueFeedback>, IVenueFeedbackRepository
{
    public VenueFeedbackRepository(ApplicationDbContext context) : base(context) { }

    public async Task<(List<VenueFeedback> Feedbacks, int Total)> GetByVenueIdAsync(
        Guid venueId,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = _dbSet
            .AsNoTracking()
            .Where(f => f.VenueId == venueId)
            .Include(f => f.Sender);

        var total = await query.CountAsync(cancellationToken);

        var feedbacks = await query
            .OrderByDescending(f => f.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (feedbacks, total);
    }
}
