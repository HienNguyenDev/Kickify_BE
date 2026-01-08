using Kickify.Api.Extensions;
using Kickify.Api.Requests;
using Kickify.Application.Features.Users.Commands.CreateUser;
using Kickify.Application.Features.Users.Commands.DeleteUser;
using Kickify.Application.Features.Users.Commands.UpdateUser;
using Kickify.Application.Features.Users.Queries.GetAllUsers;
using Kickify.Application.Features.Users.Queries.GetUserById;
using Kickify.Domain.Common;
using Kickify.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kickify.Api.Controllers
{
    [Route("api/[controller]")]
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
                AvatarUrl = request.AvatarUrl,
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
                Gender = request.Gender
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
    }
}
