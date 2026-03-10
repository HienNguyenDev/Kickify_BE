using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Abstractions.Services;
using Kickify.Domain.Common;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.VenueEvidences.Commands.DeleteVenueEvidence;

internal sealed class DeleteVenueEvidenceCommandHandler : ICommandHandler<DeleteVenueEvidenceCommand, DeleteVenueEvidenceResponse>
{
    private readonly IVenueEvidenceRepository _evidenceRepository;
    private readonly IStorageService _storageService;
    private readonly IUserContext _userContext;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteVenueEvidenceCommandHandler(
        IVenueEvidenceRepository evidenceRepository,
        IStorageService storageService,
        IUserContext userContext,
        IUnitOfWork unitOfWork)
    {
        _evidenceRepository = evidenceRepository;
        _storageService = storageService;
        _userContext = userContext;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<DeleteVenueEvidenceResponse>> Handle(DeleteVenueEvidenceCommand request, CancellationToken cancellationToken)
    {
        var evidence = await _evidenceRepository.GetByIdWithVenueAsync(request.EvidenceId, cancellationToken);
        if (evidence is null)
            return Result.Failure<DeleteVenueEvidenceResponse>(VenueErrors.EvidenceNotFound);

        if (evidence.Venue.OwnerId != _userContext.UserId)
            return Result.Failure<DeleteVenueEvidenceResponse>(VenueErrors.Unauthorized);

        // Extract object name from URL for storage deletion
        var objectName = evidence.FileUrl.Split('/').Last();
        await _storageService.DeleteAsync(objectName, cancellationToken);

        _evidenceRepository.Remove(evidence);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new DeleteVenueEvidenceResponse { EvidenceId = request.EvidenceId });
    }
}
