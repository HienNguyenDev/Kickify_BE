using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Infrastructure.Database;
using Kickify.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Kickify.Infrastructure.Repositories;

public class RoomInvitationRepository : GenericRepository<RoomInvitation>, IRoomInvitationRepository
{
    public RoomInvitationRepository(ApplicationDbContext context) : base(context) { }

    public async Task<RoomInvitation?> GetPendingInvitationAsync(Guid roomId, Guid inviteeId, CancellationToken cancellationToken = default)
    {
        return await _dbSet.FirstOrDefaultAsync(
            i => i.RoomId == roomId && i.InviteeId == inviteeId && i.Status == InvitationStatus.Pending,
            cancellationToken);
    }
}
