using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;

namespace Kickify.Application.Features.Announcements.Queries.GetAnnouncements;

internal sealed class GetAnnouncementsQueryHandler : IQueryHandler<GetAnnouncementsQuery, GetAnnouncementsResponse>
{
    private readonly IAnnouncementRepository _announcementRepository;

    public GetAnnouncementsQueryHandler(IAnnouncementRepository announcementRepository)
    {
        _announcementRepository = announcementRepository;
    }

    public async Task<Result<GetAnnouncementsResponse>> Handle(GetAnnouncementsQuery request, CancellationToken cancellationToken)
    {
        var (announcements, total) = await _announcementRepository.GetPagedAsync(
            announcementType: request.AnnouncementType,
            isActive: request.IsActive,
            page: request.Page,
            pageSize: request.PageSize,
            cancellationToken: cancellationToken);

        var dtos = announcements.Select(a => new AnnouncementDto
        {
            AnnouncementId = a.AnnouncementId,
            Title = a.Title,
            Content = a.Content,
            AnnouncementType = a.AnnouncementType.ToString(),
            Priority = a.Priority.ToString(),
            ShowFrom = a.ShowFrom,
            ShowTo = a.ShowTo,
            IsActive = a.IsActive,
            CreatedBy = a.CreatedBy,
            CreatedAt = a.CreatedAt,
            UpdatedAt = a.UpdatedAt
        }).ToList();

        return Result.Success(new GetAnnouncementsResponse
        {
            Announcements = dtos,
            Total = total,
            Page = request.Page,
            PageSize = request.PageSize
        });
    }
}
