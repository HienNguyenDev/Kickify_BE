using Kickify.Application.Abstractions.Persistence;
using Kickify.Domain.Entities;

namespace Kickify.Application.Abstractions.Repositories;

public interface IVenueFeedbackRepository : IGenericRepository<VenueFeedback>
{
    Task<(List<VenueFeedback> Feedbacks, int Total)> GetByVenueIdAsync(
        Guid venueId,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default);
}
