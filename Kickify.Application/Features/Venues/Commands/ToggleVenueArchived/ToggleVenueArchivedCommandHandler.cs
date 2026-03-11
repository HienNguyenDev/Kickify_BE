using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Abstractions.Authentication;
using Kickify.Domain.Common;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.Venues.Commands.ToggleVenueArchived;

public class ToggleVenueArchivedCommandHandler : ICommandHandler<ToggleVenueArchivedCommand, ToggleVenueArchivedResponse>
{
    private readonly IVenueRepository _venueRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContext _userContext;


    public ToggleVenueArchivedCommandHandler(
        IVenueRepository venueRepository,
        IUnitOfWork unitOfWork,
        IUserContext userContext)
    {
        _venueRepository = venueRepository;
        _unitOfWork = unitOfWork;
        _userContext = userContext;
    }

    public async Task<Result<ToggleVenueArchivedResponse>> Handle(
        ToggleVenueArchivedCommand request,
        CancellationToken cancellationToken)
    {
        var venue = await _venueRepository.GetVenueForUpdateAsync(request.VenueId, cancellationToken);
        if (venue is null)
        {
            return Result.Failure<ToggleVenueArchivedResponse>(VenueErrors.NotFound(request.VenueId));
        }
        // Verify ownership
        if (venue.OwnerId != _userContext.UserId)
        {
            return Result.Failure<ToggleVenueArchivedResponse>(VenueErrors.Unauthorized);
        }

        var previousStatus = venue.Status;

        if (venue.Status == VenueStatus.Approved)
        {
            venue.Status = VenueStatus.Archived;
        }
        else if (venue.Status == VenueStatus.Archived)
        {
            venue.Status = VenueStatus.Approved;
        }
        else
        {
            return Result.Failure<ToggleVenueArchivedResponse>(VenueErrors.CannotToggleArchived(venue.Status.ToString()));
        }

        _venueRepository.Update(venue);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success(new ToggleVenueArchivedResponse(
            venue.VenueId,
            venue.VenueName,
            previousStatus.ToString(),
            venue.Status.ToString(),
            venue.UpdatedAt
        ));
    }
}
