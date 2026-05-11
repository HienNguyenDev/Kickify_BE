using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.Venues.Commands.SubmitVenueVerification;

internal sealed class SubmitVenueVerificationCommandHandler : ICommandHandler<SubmitVenueVerificationCommand, SubmitVenueVerificationResponse>
{
    private readonly IVenueRepository _venueRepository;
    private readonly IVenuePhotoRepository _venuePhotoRepository;
    private readonly IVenueEvidenceRepository _venueEvidenceRepository;
    private readonly IUserContext _userContext;
    private readonly IUnitOfWork _unitOfWork;

    public SubmitVenueVerificationCommandHandler(
        IVenueRepository venueRepository,
        IVenuePhotoRepository venuePhotoRepository,
        IVenueEvidenceRepository venueEvidenceRepository,
        IUserContext userContext,
        IUnitOfWork unitOfWork)
    {
        _venueRepository = venueRepository;
        _venuePhotoRepository = venuePhotoRepository;
        _venueEvidenceRepository = venueEvidenceRepository;
        _userContext = userContext;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<SubmitVenueVerificationResponse>> Handle(SubmitVenueVerificationCommand request, CancellationToken cancellationToken)
    {
        var venue = await _venueRepository.GetVenueForUpdateAsync(request.VenueId, cancellationToken);
        if (venue is null)
            return Result.Failure<SubmitVenueVerificationResponse>(VenueErrors.NotFound(request.VenueId));

        if (venue.OwnerId != _userContext.UserId)
            return Result.Failure<SubmitVenueVerificationResponse>(VenueErrors.Unauthorized);

        if (venue.Status is not (VenueStatus.Draft or VenueStatus.Rejected))
            return Result.Failure<SubmitVenueVerificationResponse>(VenueErrors.InvalidVerificationStatus);

        var photos = await _venuePhotoRepository.GetPhotosByVenueIdAsync(request.VenueId, cancellationToken);
        if (!photos.Any())
            return Result.Failure<SubmitVenueVerificationResponse>(VenueErrors.InsufficientPhotos);

        var evidenceCount = await _venueEvidenceRepository.CountByVenueIdAsync(request.VenueId, cancellationToken);
        if (evidenceCount == 0)
            return Result.Failure<SubmitVenueVerificationResponse>(VenueErrors.InsufficientEvidences);

        venue.Status = VenueStatus.PendingVerification;
        venue.AdminNotes = null;

        _venueRepository.Update(venue);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new SubmitVenueVerificationResponse
        {
            VenueId = venue.VenueId,
            Status = venue.Status.ToString()
        });
    }
}
