using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;
using Microsoft.Extensions.Logging;

namespace Kickify.Application.Features.Venues.Commands.UpdateVenueStatus;

public class UpdateVenueStatusCommandHandler : ICommandHandler<UpdateVenueStatusCommand, UpdateVenueStatusResponse>
{
    private readonly IVenueRepository _venueRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UpdateVenueStatusCommandHandler> _logger;

    public UpdateVenueStatusCommandHandler(
        IVenueRepository venueRepository,
        IUnitOfWork unitOfWork,
        ILogger<UpdateVenueStatusCommandHandler> logger)
    {
        _venueRepository = venueRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<Result<UpdateVenueStatusResponse>> Handle(UpdateVenueStatusCommand request, CancellationToken cancellationToken)
    {
        // Parse status enum
        if (!Enum.TryParse<VenueStatus>(request.Status, true, out var venueStatus))
        {
            return Result.Failure<UpdateVenueStatusResponse>(VenueErrors.InvalidStatus(request.Status));
        }

        // Get venue with tracking for update
        var venue = await _venueRepository.GetVenueForUpdateAsync(request.VenueId, cancellationToken);
        if (venue is null)
        {
            return Result.Failure<UpdateVenueStatusResponse>(VenueErrors.NotFound(request.VenueId));
        }

        var previousStatus = venue.Status;

        // Update status and admin notes
        venue.Status = venueStatus;
        venue.AdminNotes = request.AdminNotes;

        _venueRepository.Update(venue);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Venue {VenueId} status updated from {PreviousStatus} to {NewStatus} by admin",
            venue.VenueId, previousStatus, venueStatus);

        return Result.Success(new UpdateVenueStatusResponse(
            venue.VenueId,
            venue.VenueName,
            previousStatus.ToString(),
            venue.Status.ToString(),
            venue.AdminNotes,
            venue.UpdatedAt
        ));
    }
}
