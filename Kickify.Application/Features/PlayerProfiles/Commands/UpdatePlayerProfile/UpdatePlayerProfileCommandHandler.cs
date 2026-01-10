using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.PlayerProfiles.Commands.UpdatePlayerProfile
{
    public class UpdatePlayerProfileCommandHandler : ICommandHandler<UpdatePlayerProfileCommand, UpdatePlayerProfileCommandResponse>
    {
        private readonly IPlayerProfileRepository _playerProfileRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdatePlayerProfileCommandHandler(
            IPlayerProfileRepository playerProfileRepository,
            IUnitOfWork unitOfWork)
        {
            _playerProfileRepository = playerProfileRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<UpdatePlayerProfileCommandResponse>> Handle(
            UpdatePlayerProfileCommand request,
            CancellationToken cancellationToken)
        {
            // Get player profile
            var profile = await _playerProfileRepository.GetByIdAsync(request.ProfileId);
            if (profile is null)
            {
                return Result.Failure<UpdatePlayerProfileCommandResponse>(
                    PlayerProfileErrors.NotFound(request.ProfileId));
            }

            // Update properties if provided
            if (request.CurrentElo.HasValue)
            {
                profile.CurrentElo = request.CurrentElo.Value;
            }

            if (request.TrustScore.HasValue)
            {
                profile.TrustScore = request.TrustScore.Value;
            }

            if (request.TotalMatches.HasValue)
            {
                profile.TotalMatches = request.TotalMatches.Value;
            }

            if (request.Wins.HasValue)
            {
                profile.Wins = request.Wins.Value;
            }

            if (request.Losses.HasValue)
            {
                profile.Losses = request.Losses.Value;
            }

            if (request.Draws.HasValue)
            {
                profile.Draws = request.Draws.Value;
            }

            if (request.MvpCount.HasValue)
            {
                profile.MvpCount = request.MvpCount.Value;
            }

            if (request.WinStreak.HasValue)
            {
                profile.WinStreak = request.WinStreak.Value;
            }

            if (request.MaxWinStreak.HasValue)
            {
                profile.MaxWinStreak = request.MaxWinStreak.Value;
            }

            if (request.AfkCount.HasValue)
            {
                profile.AfkCount = request.AfkCount.Value;
            }

            if (request.ReportCount.HasValue)
            {
                profile.ReportCount = request.ReportCount.Value;
            }

            if (request.PreferredPositions != null)
            {
                profile.PreferredPositions = request.PreferredPositions;
            }

            _playerProfileRepository.Update(profile);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = new UpdatePlayerProfileCommandResponse
            {
                ProfileId = profile.ProfileId,
                UserId = profile.UserId,
                CurrentElo = profile.CurrentElo,
                TrustScore = profile.TrustScore,
                TotalMatches = profile.TotalMatches,
                Wins = profile.Wins,
                Losses = profile.Losses,
                Draws = profile.Draws,
                MvpCount = profile.MvpCount,
                WinStreak = profile.WinStreak,
                MaxWinStreak = profile.MaxWinStreak,
                AfkCount = profile.AfkCount,
                ReportCount = profile.ReportCount,
                PreferredPositions = profile.PreferredPositions,
                UpdatedAt = profile.UpdatedAt
            };

            return Result.Success(response);
        }
    }
}
