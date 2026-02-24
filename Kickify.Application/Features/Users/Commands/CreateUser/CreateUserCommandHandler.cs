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
        private readonly IPlayerProfileRepository _playerProfileRepository;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IUnitOfWork _unitOfWork;

        public CreateUserCommandHandler(
            IUserRepository userRepository,
            IPlayerProfileRepository playerProfileRepository,
            IPasswordHasher passwordHasher,
            IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _playerProfileRepository = playerProfileRepository;
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

            // Automatically create empty PlayerProfile for the new user
            var playerProfile = new PlayerProfile
            {
                ProfileId = Guid.NewGuid(),
                UserId = user.UserId,
                CurrentElo = 1000,
                TrustScore = 100,
                TotalMatches = 0,
                Wins = 0,
                Losses = 0,
                Draws = 0,
                MvpCount = 0,
                WinStreak = 0,
                MaxWinStreak = 0,
                ReportCount = 0
            };

            await _playerProfileRepository.AddAsync(playerProfile);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = new CreateUserCommandResponse
            {
                UserId = user.UserId,
                Email = user.Email,
                FullName = user.FullName,
                Phone = user.Phone,
                Role = user.Role,
                CreatedAt = user.CreatedAt
            };

            return Result.Success(response);
        }
    }
}
