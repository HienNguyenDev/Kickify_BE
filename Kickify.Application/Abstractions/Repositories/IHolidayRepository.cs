using Kickify.Application.Abstractions.Persistence;
using Kickify.Domain.Entities;

namespace Kickify.Application.Abstractions.Repositories;

public interface IHolidayRepository : IGenericRepository<Holiday>
{
    Task<IReadOnlyList<Holiday>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Holiday>> GetByIdsAsync(IEnumerable<Guid> holidayIds, CancellationToken cancellationToken = default);
    Task<Holiday?> GetByDateAsync(DateTime date, CancellationToken cancellationToken = default);
    Task<bool> ExistsByDateAsync(DateTime date, Guid? excludeHolidayId = null, CancellationToken cancellationToken = default);
    Task<List<DateTime>> GetExistingDatesAsync(IEnumerable<DateTime> dates, CancellationToken cancellationToken = default);
    Task AddRangeAsync(IEnumerable<Holiday> holidays);
    Task<(IReadOnlyList<Holiday> Items, int TotalCount)> SearchHolidaysAsync(
        string? keyword,
        int? year,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task<bool> HardDeleteByIdAsync(Guid holidayId, CancellationToken cancellationToken = default);
}