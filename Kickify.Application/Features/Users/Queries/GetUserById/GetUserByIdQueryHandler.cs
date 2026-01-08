using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.Users.Queries.GetUserById
{
    public class GetUserByIdQueryHandler : IQueryHandler<GetUserByIdQuery, GetUserByIdQueryResponse>
    {
        private readonly IUserRepository _userRepository;

        public GetUserByIdQueryHandler(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<Result<GetUserByIdQueryResponse>> Handle(
            GetUserByIdQuery request,
            CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(request.UserId);

                //GetUserWithDetailsAsync(request.UserId);
            if (user is null)
            {
                return Result.Failure<GetUserByIdQueryResponse>(UserErrors.NotFound(request.UserId));
            }

            var response = new GetUserByIdQueryResponse
            {
                UserId = user.UserId,
                Email = user.Email,
                FullName = user.FullName,
                Phone = user.Phone,
                AvatarUrl = user.AvatarUrl,
                Bio = user.Bio,
                DateOfBirth = user.DateOfBirth,
                Gender = user.Gender,
                Role = user.Role,
                IsEmailVerified = user.IsEmailVerified,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            };

            return Result.Success(response);
        }
    }
}
