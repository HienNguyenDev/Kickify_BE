using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.PlayerProfiles.Commands.DeletePlayerProfile
{
    public class DeletePlayerProfileCommandHandler : ICommandHandler<DeletePlayerProfileCommand, DeletePlayerProfileCommandResponse>
    {
        private readonly IPlayerProfileRepository _playerProfileRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeletePlayerProfileCommandHandler(
            IPlayerProfileRepository playerProfileRepository,
            IUnitOfWork unitOfWork)
        {
            _playerProfileRepository = playerProfileRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<DeletePlayerProfileCommandResponse>> Handle(
            DeletePlayerProfileCommand request,
            CancellationToken cancellationToken)
        {
            // Get player profile
            var profile = await _playerProfileRepository.GetByIdAsync(request.ProfileId);
            if (profile is null)
            {
                return Result.Failure<DeletePlayerProfileCommandResponse>(
                    PlayerProfileErrors.NotFound(request.ProfileId));
            }

            // Soft delete the profile
            _playerProfileRepository.Remove(profile);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = new DeletePlayerProfileCommandResponse
            {
                ProfileId = profile.ProfileId,
                IsDeleted = true,
                DeletedAt = DateTime.UtcNow
            };

            return Result.Success(response);
        }
    }
}
