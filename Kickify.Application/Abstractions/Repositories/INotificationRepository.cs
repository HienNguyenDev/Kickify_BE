using Kickify.Application.Abstractions.Persistence;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;

namespace Kickify.Application.Abstractions.Repositories;

public interface INotificationRepository : IGenericRepository<Notification>
{
    Task<(List<Notification> Notifications, int Total)> GetByUserIdAsync(
        Guid userId,
        NotificationType? type = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);
}
