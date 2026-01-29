using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.MatchPresets.Commands.DeleteMatchPreset
{
    public class DeleteMatchPresetCommandHandler : ICommandHandler<DeleteMatchPresetCommand, DeleteMatchPresetResponse>
    {
        private readonly IMatchPresetRepository _matchPresetRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteMatchPresetCommandHandler(
            IMatchPresetRepository matchPresetRepository,
            IUnitOfWork unitOfWork)
        {
            _matchPresetRepository = matchPresetRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<DeleteMatchPresetResponse>> Handle(DeleteMatchPresetCommand request, CancellationToken cancellationToken)
        {
            // Get preset
            var preset = await _matchPresetRepository.GetByIdAsync(request.PresetId, cancellationToken);
            if (preset == null)
            {
                return Result.Failure<DeleteMatchPresetResponse>(MatchPresetErrors.NotFound(request.PresetId));
            }

            // Check ownership
            if (preset.UserId != request.UserId)
            {
                return Result.Failure<DeleteMatchPresetResponse>(MatchPresetErrors.Unauthorized);
            }

            _matchPresetRepository.Remove(preset);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(new DeleteMatchPresetResponse(
                request.PresetId,
                "Match preset deleted successfully"
            ));
        }
    }
}
