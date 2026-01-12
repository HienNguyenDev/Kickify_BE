using Kickify.Application.Abstractions.Persistence;
using Kickify.Domain.Entities;

namespace Kickify.Application.Abstractions.Repositories
{
    public interface IFieldRepository : IGenericRepository<Field>
    {
        Task<Field?> GetFieldWithVenueAsync(Guid fieldId, CancellationToken cancellationToken = default);
        Task<IEnumerable<Field>> GetFieldsByVenueAsync(Guid venueId, CancellationToken cancellationToken = default);
        Task<bool> IsFieldAvailableAsync(
            Guid fieldId, 
            DateTime date, 
            TimeSpan startTime, 
            TimeSpan endTime, 
            CancellationToken cancellationToken = default);
    }
}
