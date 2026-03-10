using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Entities;
using Kickify.Domain.Event;
using MediatR;

namespace Kickify.Application.Features.Announcements.Commands.CreateAnnouncement;

internal sealed class CreateAnnouncementCommandHandler : ICommandHandler<CreateAnnouncementCommand, CreateAnnouncementResponse>
{
    private readonly IAnnouncementRepository _announcementRepository;
    private readonly IUserContext _userContext;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPublisher _publisher;

    public CreateAnnouncementCommandHandler(
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

    public async Task<Result<CreateAnnouncementResponse>> Handle(CreateAnnouncementCommand request, CancellationToken cancellationToken)
    {
        var announcement = new Announcement
        {
            AnnouncementId = Guid.NewGuid(),
            Title = request.Title,
            Content = request.Content,
            AnnouncementType = request.AnnouncementType,
            Priority = request.Priority,
            ShowFrom = request.ShowFrom,
            ShowTo = request.ShowTo,
            IsActive = true,
            CreatedBy = _userContext.UserId
        };

        await _announcementRepository.AddAsync(announcement);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        await _publisher.Publish(new AnnouncementCreatedDomainEvent(
            announcement.AnnouncementId,
            announcement.CreatedBy,
            announcement.Title,
            announcement.Content,
            announcement.AnnouncementType), cancellationToken);

        return Result.Success(new CreateAnnouncementResponse
        {
            AnnouncementId = announcement.AnnouncementId,
            Title = announcement.Title,
            AnnouncementType = announcement.AnnouncementType,
            ShowFrom = announcement.ShowFrom,
            ShowTo = announcement.ShowTo
        });
    }
}
