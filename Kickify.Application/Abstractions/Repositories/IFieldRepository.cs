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

        /// <summary>
        /// Get field with venue for update (WITH tracking)
        /// </summary>
        Task<Field?> GetFieldWithVenueForUpdateAsync(
            Guid fieldId,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get paged fields with optional filters
        /// </summary>
        Task<(IEnumerable<Field> Fields, int Total)> GetFieldsPagedAsync(
            Kickify.Domain.Enums.FieldType? fieldType = null,
            bool? isActive = null,
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get paged fields by owner
        /// </summary>
        Task<(IEnumerable<Field> Fields, int Total)> GetFieldsByOwnerPagedAsync(
            Guid ownerId,
            int page = 1,
            int pageSize = 10,
            CancellationToken cancellationToken = default);
    }
}
