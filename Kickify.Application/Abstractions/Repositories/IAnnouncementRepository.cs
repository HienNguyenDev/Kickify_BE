using Kickify.Application.Abstractions.Persistence;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;

namespace Kickify.Application.Abstractions.Repositories;

public interface IAnnouncementRepository : IGenericRepository<Announcement>
{
    Task<(List<Announcement> Announcements, int Total)> GetPagedAsync(
        AnnouncementType? announcementType = null,
        bool? isActive = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default);

    Task<Announcement?> GetByIdAsync(Guid announcementId, CancellationToken cancellationToken = default);
}
