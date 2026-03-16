using Kickify.Application.Abstractions.Persistence;
using Kickify.Domain.Entities;

namespace Kickify.Application.Abstractions.Repositories;

public interface IHolidayRepository : IGenericRepository<Holiday>
{
    Task<IReadOnlyList<Holiday>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Holiday>> GetByIdsAsync(IEnumerable<Guid> holidayIds, CancellationToken cancellationToken = default);
    Task<Holiday?> GetByDateAsync(DateTime date, CancellationToken cancellationToken = default);
}