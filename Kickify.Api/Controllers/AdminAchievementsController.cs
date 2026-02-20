using Kickify.Api.Extensions;
using Kickify.Api.Requests;
using Kickify.Application.Abstractions.Services;
using Kickify.Application.Features.Achievements.Commands.CreateAchievement;
using Kickify.Application.Features.Achievements.Commands.DeleteAchievement;
using Kickify.Application.Features.Achievements.Commands.UpdateAchievement;
using Kickify.Application.Features.Achievements.Queries.GetAchievementById;
using Kickify.Application.Features.Achievements.Queries.GetAllAchievements;
using Kickify.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kickify.Api.Controllers;

[Route("api/admin/achievements")]
[ApiController]
[Authorize(Roles = "Admin")]
public class AdminAchievementsController : ControllerBase
{
    private readonly ISender _mediator;

    public AdminAchievementsController(ISender mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IResult> GetAllAchievements(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAllAchievementsQuery(page, pageSize);
        Result<GetAllAchievementsResponse> result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    [HttpGet("{achievementId:guid}")]
    public async Task<IResult> GetAchievementById(Guid achievementId, CancellationToken cancellationToken)
    {
        var query = new GetAchievementByIdQuery(achievementId);
        Result<GetAchievementByIdResponse> result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    [HttpPost]
    [Consumes("multipart/form-data")]
    public async Task<IResult> CreateAchievement([FromForm] CreateAchievementRequest request, CancellationToken cancellationToken)
    {
        FileUploadRequest? fileUpload = null;
        if (request.IconFile is not null)
        {
            fileUpload = new FileUploadRequest(
                request.IconFile.OpenReadStream(),
                request.IconFile.FileName,
                request.IconFile.ContentType,
                request.IconFile.Length);
        }

        var command = new CreateAchievementCommand
        {
            Name = request.Name,
            Description = request.Description,
            CriteriaType = request.CriteriaType,
            CriteriaValue = request.CriteriaValue,
            IconFile = fileUpload
        };

        Result<CreateAchievementResponse> result = await _mediator.Send(command, cancellationToken);
        return result.MatchCreated(response => $"/api/admin/achievements/{response.AchievementId}");
    }

    [HttpPut("{achievementId:guid}")]
    [Consumes("multipart/form-data")]
    public async Task<IResult> UpdateAchievement(Guid achievementId, [FromForm] UpdateAchievementRequest request, CancellationToken cancellationToken)
    {
        FileUploadRequest? fileUpload = null;
        if (request.IconFile is not null)
        {
            fileUpload = new FileUploadRequest(
                request.IconFile.OpenReadStream(),
                request.IconFile.FileName,
                request.IconFile.ContentType,
                request.IconFile.Length);
        }

        var command = new UpdateAchievementCommand
        {
            AchievementId = achievementId,
            Name = request.Name,
            Description = request.Description,
            CriteriaType = request.CriteriaType,
            CriteriaValue = request.CriteriaValue,
            IconFile = fileUpload
        };

        Result<UpdateAchievementResponse> result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    [HttpDelete("{achievementId:guid}")]
    public async Task<IResult> DeleteAchievement(Guid achievementId, CancellationToken cancellationToken)
    {
        var command = new DeleteAchievementCommand(achievementId);
        Result<DeleteAchievementResponse> result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }
}
