using Kickify.Api.Extensions;
using Kickify.Api.Requests;
using Kickify.Application.Features.Announcements.Commands.CreateAnnouncement;
using Kickify.Application.Features.Announcements.Commands.DeleteAnnouncement;
using Kickify.Application.Features.Announcements.Commands.UpdateAnnouncement;
using Kickify.Application.Features.Announcements.Queries.GetAnnouncements;
using Kickify.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kickify.Api.Controllers;

[ApiController]
[Route("api/announcements")]
[Authorize(Roles = "Admin")]
public class AnnouncementsController : ControllerBase
{
    private readonly ISender _sender;

    public AnnouncementsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Get announcements with optional filters
    /// </summary>
    [HttpGet]
    public async Task<IResult> GetAnnouncements(
        [FromQuery] AnnouncementType? announcementType = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAnnouncementsQuery
        {
            AnnouncementType = announcementType,
            IsActive = isActive,
            Page = page,
            PageSize = pageSize
        };
        var result = await _sender.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// [Admin] Create a new announcement and push notification to all users
    /// </summary>
    [HttpPost]
    public async Task<IResult> CreateAnnouncement(
        [FromBody] CreateAnnouncementRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateAnnouncementCommand
        {
            Title = request.Title,
            Content = request.Content,
            AnnouncementType = request.AnnouncementType,
            Priority = request.Priority,
            ShowFrom = request.ShowFrom,
            ShowTo = request.ShowTo
        };
        var result = await _sender.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// [Admin] Update an announcement and push notification to all users
    /// </summary>
    [HttpPut("{announcementId:guid}")]
    public async Task<IResult> UpdateAnnouncement(
        Guid announcementId,
        [FromBody] UpdateAnnouncementRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateAnnouncementCommand
        {
            AnnouncementId = announcementId,
            Title = request.Title,
            Content = request.Content,
            AnnouncementType = request.AnnouncementType,
            Priority = request.Priority,
            ShowFrom = request.ShowFrom,
            ShowTo = request.ShowTo,
            IsActive = request.IsActive
        };
        var result = await _sender.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// [Admin] Delete an announcement
    /// </summary>
    [HttpDelete("{announcementId:guid}")]
    public async Task<IResult> DeleteAnnouncement(
        Guid announcementId,
        CancellationToken cancellationToken)
    {
        var command = new DeleteAnnouncementCommand { AnnouncementId = announcementId };
        var result = await _sender.Send(command, cancellationToken);
        return result.MatchOk();
    }
}
