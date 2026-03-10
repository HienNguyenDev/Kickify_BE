using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Errors;
using Kickify.Domain.Event;
using MediatR;

namespace Kickify.Application.Features.Announcements.Commands.UpdateAnnouncement;

internal sealed class UpdateAnnouncementCommandHandler : ICommandHandler<UpdateAnnouncementCommand, UpdateAnnouncementResponse>
{
    private readonly IAnnouncementRepository _announcementRepository;
    private readonly IUserContext _userContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPublisher _publisher;

    public UpdateAnnouncementCommandHandler(
        IAnnouncementRepository announcementRepository,
        IUserContext userContext,
        IUnitOfWork unitOfWork,
        IPublisher publisher)
    {
        _announcementRepository = announcementRepository;
        _userContext = userContext;
        _unitOfWork = unitOfWork;
        _publisher = publisher;
    }

    public async Task<Result<UpdateAnnouncementResponse>> Handle(UpdateAnnouncementCommand request, CancellationToken cancellationToken)
    {
        var announcement = await _announcementRepository.GetByIdAsync(request.AnnouncementId, cancellationToken);
        if (announcement is null)
            return Result.Failure<UpdateAnnouncementResponse>(AnnouncementErrors.NotFound(request.AnnouncementId));

        announcement.Title = request.Title;
        announcement.Content = request.Content;
        announcement.AnnouncementType = request.AnnouncementType;
        announcement.Priority = request.Priority;
        announcement.ShowFrom = request.ShowFrom;
        announcement.ShowTo = request.ShowTo;
        announcement.IsActive = request.IsActive;

        _announcementRepository.Update(announcement);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _publisher.Publish(new AnnouncementUpdatedDomainEvent(
            announcement.AnnouncementId,
            _userContext.UserId,
            announcement.Title,
            announcement.Content,
            announcement.AnnouncementType), cancellationToken);

        return Result.Success(new UpdateAnnouncementResponse
        {
            AnnouncementId = announcement.AnnouncementId,
            Title = announcement.Title,
            AnnouncementType = announcement.AnnouncementType,
            IsActive = announcement.IsActive,
            ShowFrom = announcement.ShowFrom,
            ShowTo = announcement.ShowTo
        });
    }
}
