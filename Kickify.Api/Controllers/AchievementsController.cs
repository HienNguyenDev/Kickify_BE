using Kickify.Api.Extensions;
using Kickify.Api.Requests;
using Kickify.Application.Abstractions.Services;
using Kickify.Application.Features.Achievements.Commands.ClaimMyAchievement;
using Kickify.Application.Features.Achievements.Commands.CreateAchievement;
using Kickify.Application.Features.Achievements.Commands.DeleteAchievement;
using Kickify.Application.Features.Achievements.Commands.UpdateAchievement;
using Kickify.Application.Features.Achievements.Queries.GetAchievementById;
using Kickify.Application.Features.Achievements.Queries.GetAllAchievements;
using Kickify.Application.Features.Achievements.Queries.GetMyAchievements;
using Kickify.Domain.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kickify.Api.Controllers;

[Route("api/achievements")]
[ApiController]
[Authorize]
public class AchievementsController : ControllerBase
{
    private readonly ISender _mediator;

    public AchievementsController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get current user's achievements with unlock progress
    /// </summary>
    [HttpGet("me")]
    public async Task<IResult> GetMyAchievements(CancellationToken cancellationToken)
    {
        var query = new GetMyAchievementsQuery();
        Result<GetMyAchievementsResponse> result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// Claim an achievement for current user when conditions are met
    /// </summary>
    [HttpPost("me/claim/{achievementId:guid}")]
    public async Task<IResult> ClaimMyAchievement(Guid achievementId, CancellationToken cancellationToken)
    {
        var command = new ClaimMyAchievementCommand(achievementId);
        Result<ClaimMyAchievementResponse> result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// Get all achievements (Admin only)
    /// </summary>
    [Authorize(Roles = "Admin")]
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

    /// <summary>
    /// Get achievement by ID (Admin only)
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpGet("{achievementId:guid}")]
    public async Task<IResult> GetAchievementById(Guid achievementId, CancellationToken cancellationToken)
    {
        var query = new GetAchievementByIdQuery(achievementId);
        Result<GetAchievementByIdResponse> result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// Create a new achievement (Admin only)
    /// </summary>
    [Authorize(Roles = "Admin")]
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
        return result.MatchCreated(response => $"/api/achievements/{response.AchievementId}");
    }

    /// <summary>
    /// Update an achievement (Admin only)
    /// </summary>
    [Authorize(Roles = "Admin")]
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

    /// <summary>
    /// Delete an achievement (Admin only)
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpDelete("{achievementId:guid}")]
    public async Task<IResult> DeleteAchievement(Guid achievementId, CancellationToken cancellationToken)
    {
        var command = new DeleteAchievementCommand(achievementId);
        Result<DeleteAchievementResponse> result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }
}
