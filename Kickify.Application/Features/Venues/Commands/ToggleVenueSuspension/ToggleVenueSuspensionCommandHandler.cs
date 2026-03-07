using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.Venues.Commands.ToggleVenueSuspension;

public class ToggleVenueSuspensionCommandHandler : ICommandHandler<ToggleVenueSuspensionCommand, ToggleVenueSuspensionResponse>
{
    private readonly IVenueRepository _venueRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContext _userContext;

    public ToggleVenueSuspensionCommandHandler(
        IVenueRepository venueRepository,
        IUnitOfWork unitOfWork,
        IUserContext userContext)
    {
        _venueRepository = venueRepository;
        _unitOfWork = unitOfWork;
        _userContext = userContext;
    }

    public async Task<Result<ToggleVenueSuspensionResponse>> Handle(
        ToggleVenueSuspensionCommand request,
        CancellationToken cancellationToken)
    {
        var venue = await _venueRepository.GetVenueForUpdateAsync(request.VenueId, cancellationToken);
        if (venue is null)
        {
            return Result.Failure<ToggleVenueSuspensionResponse>(VenueErrors.NotFound(request.VenueId));
        }

        // Verify ownership
        if (venue.OwnerId != _userContext.UserId)
        {
            return Result.Failure<ToggleVenueSuspensionResponse>(VenueErrors.Unauthorized);
        }

        var previousStatus = venue.Status;

        if (venue.Status == VenueStatus.Approved)
        {
            // Suspend the venue
            venue.Status = VenueStatus.Suspended;
        }
        else if (venue.Status == VenueStatus.Suspended)
        {
            // Unsuspend -> back to Approved
            venue.Status = VenueStatus.Approved;
        }
        else
        {
            // Only Approved venues can be suspended, only Suspended can be unsuspended
            return Result.Failure<ToggleVenueSuspensionResponse>(VenueErrors.CannotToggleSuspension(venue.Status.ToString()));
        }

        _venueRepository.Update(venue);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new ToggleVenueSuspensionResponse(
            venue.VenueId,
            venue.VenueName,
            previousStatus.ToString(),
            venue.Status.ToString(),
            venue.UpdatedAt
        ));
    }
}
