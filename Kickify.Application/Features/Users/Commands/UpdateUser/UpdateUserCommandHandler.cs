using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.Users.Commands.UpdateUser
{
    public class UpdateUserCommandHandler : ICommandHandler<UpdateUserCommand, UpdateUserCommandResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateUserCommandHandler(
            IUserRepository userRepository,
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<UpdateUserCommandResponse>> Handle(
            UpdateUserCommand request,
            CancellationToken cancellationToken)
        {
            // Get user by id
            var user = await _userRepository.GetByIdAsync(request.UserId);
            if (user is null)
            {
                return Result.Failure<UpdateUserCommandResponse>(UserErrors.NotFound(request.UserId));
            }

            // Update user properties
            user.FullName = request.FullName;
            user.Phone = request.Phone;
            user.AvatarUrl = request.AvatarUrl;
            user.Bio = request.Bio;
            user.DateOfBirth = request.DateOfBirth;
            user.Gender = request.Gender;
            user.UpdatedAt = DateTime.UtcNow;

            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = new UpdateUserCommandResponse
            {
                UserId = user.UserId,
                Email = user.Email,
                FullName = user.FullName,
                Phone = user.Phone,
                AvatarUrl = user.AvatarUrl,
                Bio = user.Bio,
                DateOfBirth = user.DateOfBirth,
                Gender = user.Gender,
                UpdatedAt = user.UpdatedAt
            };

            return Result.Success(response);
        }
    }
}
