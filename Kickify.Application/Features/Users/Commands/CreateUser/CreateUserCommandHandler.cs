using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Entities;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.Users.Commands.CreateUser
{
    public class CreateUserCommandHandler : ICommandHandler<CreateUserCommand, CreateUserCommandResponse>
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IUnitOfWork _unitOfWork;

        public CreateUserCommandHandler(
            IUserRepository userRepository,
            IPasswordHasher passwordHasher,
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _passwordHasher = passwordHasher;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<CreateUserCommandResponse>> Handle(
            CreateUserCommand request,
            CancellationToken cancellationToken)
        {
            // Check if email already exists
            var emailExists = await _userRepository.IsEmailExistsAsync(request.Email);
            if (emailExists)
            {
                return Result.Failure<CreateUserCommandResponse>(UserErrors.EmailAlreadyExists);
            }

            // Create new user
            var user = new User
            {
                UserId = Guid.NewGuid(),
                Email = request.Email,
                PasswordHash = _passwordHasher.Hash(request.Password),
                FullName = request.FullName,
                Phone = request.Phone,
                AvatarUrl = request.AvatarUrl,
                Bio = request.Bio,
                DateOfBirth = request.DateOfBirth,
                Gender = request.Gender,
                Role = request.Role,
                IsEmailVerified = false,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _userRepository.AddAsync(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = new CreateUserCommandResponse
            {
                UserId = user.UserId,
                Email = user.Email,
                FullName = user.FullName,
                Phone = user.Phone,
                AvatarUrl = user.AvatarUrl,
                Role = user.Role,
                CreatedAt = user.CreatedAt
            };

            return Result.Success(response);
        }
    }
}
