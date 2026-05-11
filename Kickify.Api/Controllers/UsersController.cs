using Kickify.Api.Extensions;
using Kickify.Api.Requests;
using Kickify.Application.Abstractions.Services;
using Kickify.Application.Features.Users.Commands.BanUnbanUser;
using Kickify.Application.Features.Users.Commands.BanUser;
using Kickify.Application.Features.Users.Commands.CreateUser;
using Kickify.Application.Features.Users.Commands.DeleteUser;
using Kickify.Application.Features.Users.Commands.UpdateFcmToken;
using Kickify.Application.Features.Users.Commands.UpdateUser;
using Kickify.Application.Features.Users.Commands.UploadUserAvatar;
using Kickify.Application.Features.Users.Queries.GetAllUsers;
using Kickify.Application.Features.Users.Queries.GetUserById;
using Kickify.Domain.Common;
using Kickify.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kickify.Api.Controllers;

[Route("api/users")]
[ApiController]
public class UsersController : ControllerBase
{
    private readonly ISender _mediator;

    public UsersController(ISender mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get all users with pagination and filters
    /// </summary>
    [HttpGet]
    public async Task<IResult> GetAllUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] UserRole? role = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAllUsersQuery
        {
            Page = page,
            PageSize = pageSize,
            Role = role,
            IsActive = isActive,
            SearchTerm = searchTerm
        };

        Result<GetAllUsersQueryResponse> result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// Get user by ID
    /// </summary>
    [HttpGet("{userId:guid}")]
    public async Task<IResult> GetUserById(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var query = new GetUserByIdQuery { UserId = userId };
        Result<GetUserByIdQueryResponse> result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// Create new user
    /// </summary>
    [HttpPost]
    public async Task<IResult> CreateUser(
        [FromBody] CreateUserRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateUserCommand
        {
            Email = request.Email,
            Password = request.Password,
            FullName = request.FullName,
            Phone = request.Phone,
            Bio = request.Bio,
            DateOfBirth = request.DateOfBirth,
            Gender = request.Gender,
            Role = request.Role
        };

        Result<CreateUserCommandResponse> result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// Update user information
    /// </summary>
    [HttpPut("{userId:guid}")]
    public async Task<IResult> UpdateUser(
        Guid userId,
        [FromBody] UpdateUserRequest request,
        CancellationToken cancellationToken)
    {
        var command = new UpdateUserCommand
        {
            UserId = userId,
            FullName = request.FullName,
            Phone = request.Phone,
            AvatarUrl = request.AvatarUrl,
            Bio = request.Bio,
            DateOfBirth = request.DateOfBirth,
            Gender = request.Gender,
            PreferredPositions = request.PreferredPositions,
            ShirtNumber = request.ShirtNumber,
            PreferredFoot = request.PreferredFoot
        };

        Result<UpdateUserCommandResponse> result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// Delete user (soft delete)
    /// </summary>
    [HttpDelete("{userId:guid}")]
    public async Task<IResult> DeleteUser(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var command = new DeleteUserCommand { UserId = userId };
        Result<DeleteUserCommandResponse> result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// [Admin] Ban user with specific duration (1, 3, 7, 30 days or permanent)
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPost("{userId:guid}/ban")]
    public async Task<IResult> BanUser(
        Guid userId,
        [FromBody] BanUserRequest request,
        CancellationToken cancellationToken)
    {
        var command = new BanUserCommand(userId, request.Duration, request.Reason);
        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// [Admin] Unban user
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpDelete("{userId:guid}/ban")]
    public async Task<IResult> UnbanUser(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var command = new BanUnbanUserCommand(userId, IsActive: true);
        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// Get all banned users — Admin only
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpGet("banned")]
    public async Task<IResult> GetBannedUsers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null,
        CancellationToken cancellationToken = default)
    {
        var query = new GetAllUsersQuery
        {
            Page = page,
            PageSize = pageSize,
            IsActive = false,
            SearchTerm = searchTerm
        };
        Result<GetAllUsersQueryResponse> result = await _mediator.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// Upload user avatar
    /// </summary>
    [HttpPost("avatar")]
    [Authorize]
    [Consumes("multipart/form-data")]
    public async Task<IResult> UploadAvatar(IFormFile file, CancellationToken cancellationToken)
    {
        var fileUploadRequest = new FileUploadRequest(
            file.OpenReadStream(), file.FileName, file.ContentType, file.Length);
        var command = new UploadUserAvatarCommand { File = fileUploadRequest };
        Result<UploadUserAvatarCommandResponse> result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// Cập nhật FCM token cho push notification
    /// </summary>
    [HttpPut("fcm-token")]
    [Authorize]
    public async Task<IResult> UpdateFcmToken(
        [FromBody] UpdateFcmTokenCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);
        return result.MatchOk();
    }
}
