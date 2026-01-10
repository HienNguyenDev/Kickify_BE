using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Entities;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.PlayerProfiles.Commands.CreatePlayerProfile
{
    public class CreatePlayerProfileCommandHandler : ICommandHandler<CreatePlayerProfileCommand, CreatePlayerProfileCommandResponse>
    {
        private readonly IPlayerProfileRepository _playerProfileRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreatePlayerProfileCommandHandler(
            IPlayerProfileRepository playerProfileRepository,
            IUserRepository userRepository,
            IUnitOfWork unitOfWork)
        {
            _playerProfileRepository = playerProfileRepository;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<CreatePlayerProfileCommandResponse>> Handle(
            CreatePlayerProfileCommand request,
            CancellationToken cancellationToken)
        {
            // Check if user exists
            var user = await _userRepository.GetByIdAsync(request.UserId);
            if (user is null)
            {
                return Result.Failure<CreatePlayerProfileCommandResponse>(PlayerProfileErrors.UserNotFound);
            }

            // Check if player profile already exists for this user
            var profileExists = await _playerProfileRepository.ExistsByUserIdAsync(request.UserId, cancellationToken);
            if (profileExists)
            {
                return Result.Failure<CreatePlayerProfileCommandResponse>(
                    PlayerProfileErrors.AlreadyExists(request.UserId));
            }

            // Create new player profile
            var profile = new PlayerProfile
            {
                ProfileId = Guid.NewGuid(),
                UserId = request.UserId,
                CurrentElo = request.CurrentElo ?? 1000,
                TrustScore = request.TrustScore ?? 100.00m,
                TotalMatches = 0,
                Wins = 0,
                Losses = 0,
                Draws = 0,
                MvpCount = 0,
                WinStreak = 0,
                MaxWinStreak = 0,
                AfkCount = 0,
                ReportCount = 0,
                PreferredPositions = request.PreferredPositions
            };

            await _playerProfileRepository.AddAsync(profile);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = new CreatePlayerProfileCommandResponse
            {
                ProfileId = profile.ProfileId,
                UserId = profile.UserId,
                CurrentElo = profile.CurrentElo,
                TrustScore = profile.TrustScore,
                TotalMatches = profile.TotalMatches,
                PreferredPositions = profile.PreferredPositions,
                CreatedAt = profile.CreatedAt
            };

            return Result.Success(response);
        }
    }
}
