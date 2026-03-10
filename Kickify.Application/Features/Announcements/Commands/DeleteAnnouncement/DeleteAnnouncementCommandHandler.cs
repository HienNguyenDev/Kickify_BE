using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.Announcements.Commands.DeleteAnnouncement;

internal sealed class DeleteAnnouncementCommandHandler : ICommandHandler<DeleteAnnouncementCommand, DeleteAnnouncementResponse>
{
    private readonly IAnnouncementRepository _announcementRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteAnnouncementCommandHandler(
        IAnnouncementRepository announcementRepository,
        IUnitOfWork unitOfWork)
    {
        _announcementRepository = announcementRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<DeleteAnnouncementResponse>> Handle(DeleteAnnouncementCommand request, CancellationToken cancellationToken)
    {
        var announcement = await _announcementRepository.GetByIdAsync(request.AnnouncementId, cancellationToken);
        if (announcement is null)
            return Result.Failure<DeleteAnnouncementResponse>(AnnouncementErrors.NotFound(request.AnnouncementId));

        _announcementRepository.Remove(announcement);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new DeleteAnnouncementResponse { AnnouncementId = request.AnnouncementId });
    }
}
