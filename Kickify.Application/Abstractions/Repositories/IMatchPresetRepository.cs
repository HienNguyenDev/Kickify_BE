using Kickify.Application.Abstractions.Persistence;
using Kickify.Domain.Entities;

namespace Kickify.Application.Abstractions.Repositories
{
    public interface IMatchPresetRepository : IGenericRepository<MatchPreset>
    {
        Task<MatchPreset?> GetByIdAsync(Guid presetId, CancellationToken cancellationToken = default);
        Task<MatchPreset?> GetByIdWithDetailsAsync(Guid presetId, CancellationToken cancellationToken = default);
        Task<List<MatchPreset>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<(IEnumerable<MatchPreset> Presets, int Total)> GetAllPagedAsync(
            int page,
            int pageSize,
            CancellationToken cancellationToken = default);
        Task<(IEnumerable<MatchPreset> Presets, int Total)> GetByUserIdPagedAsync(
            Guid userId,
            int page,
            int pageSize,
            CancellationToken cancellationToken = default);
    }
}
