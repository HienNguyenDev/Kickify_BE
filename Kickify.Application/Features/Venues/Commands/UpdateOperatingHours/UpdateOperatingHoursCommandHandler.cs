using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.Venues.Commands.UpdateOperatingHours;

public class UpdateOperatingHoursCommandHandler : ICommandHandler<UpdateOperatingHoursCommand, UpdateOperatingHoursResponse>
{
    private readonly IVenueRepository _venueRepository;
    private readonly IVenueOperatingHourRepository _operatingHourRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IUserContext _userContext;

    public UpdateOperatingHoursCommandHandler(
        IVenueRepository venueRepository,
        IVenueOperatingHourRepository operatingHourRepository,
        IUnitOfWork unitOfWork,
        IUserContext userContext)
    {
        _venueRepository = venueRepository;
        _operatingHourRepository = operatingHourRepository;
        _unitOfWork = unitOfWork;
        _userContext = userContext;
    }

    public async Task<Result<UpdateOperatingHoursResponse>> Handle(
        UpdateOperatingHoursCommand request,
        CancellationToken cancellationToken)
    {
        // Check if venue exists
        var venue = await _venueRepository.GetByIdAsync(request.VenueId);
        if (venue is null)
        {
            return Result.Failure<UpdateOperatingHoursResponse>(VenueErrors.NotFound(request.VenueId));
        }

        // Check if user is the owner
        if (venue.OwnerId != _userContext.UserId)
        {
            return Result.Failure<UpdateOperatingHoursResponse>(VenueErrors.Unauthorized);
        }

        // Get existing operating hours for this venue
        var existingHours = await _operatingHourRepository.GetByVenueIdAsync(request.VenueId, cancellationToken);

        var resultHours = new List<VenueOperatingHour>();
        var newHoursToAdd = new List<VenueOperatingHour>();

        foreach (var item in request.OperatingHours)
        {
            var dayOfWeek = (DayOfWeekEnum)item.DayOfWeek;
            var existingHour = existingHours.FirstOrDefault(h => h.DayOfWeek == dayOfWeek);

            // Logic:
            // - null: Keep existing (no change)
            // - "": Delete/Clear (set IsClosed = true, clear times)
            // - valid time: Update with new value

            if (item.OpenTime == null && item.CloseTime == null)
            {
                // Keep existing - no changes
                if (existingHour != null)
                {
                    resultHours.Add(existingHour);
                }
                else
                {
                    // Create new with IsClosed = true if no existing
                    var newHour = new VenueOperatingHour
                    {
                        HoursId = Guid.NewGuid(),
                        VenueId = request.VenueId,
                        DayOfWeek = dayOfWeek,
                        OpenTime = null,
                        CloseTime = null,
                        IsClosed = true
                    };
                    newHoursToAdd.Add(newHour);
                    resultHours.Add(newHour);
                }
            }
            else if (item.OpenTime == string.Empty || item.CloseTime == string.Empty)
            {
                // Clear/Delete - mark as closed
                if (existingHour != null)
                {
                    existingHour.OpenTime = null;
                    existingHour.CloseTime = null;
                    existingHour.IsClosed = true;
                    _operatingHourRepository.Update(existingHour);
                    resultHours.Add(existingHour);
                }
                else
                {
                    var newHour = new VenueOperatingHour
                    {
                        HoursId = Guid.NewGuid(),
                        VenueId = request.VenueId,
                        DayOfWeek = dayOfWeek,
                        OpenTime = null,
                        CloseTime = null,
                        IsClosed = true
                    };
                    newHoursToAdd.Add(newHour);
                    resultHours.Add(newHour);
                }
            }
            else
            {
                // Update with new values
                var openTime = TimeSpan.TryParse(item.OpenTime, out var ot) ? ot : (TimeSpan?)null;
                var closeTime = TimeSpan.TryParse(item.CloseTime, out var ct) ? ct : (TimeSpan?)null;

                if (existingHour != null)
                {
                    existingHour.OpenTime = openTime;
                    existingHour.CloseTime = closeTime;
                    existingHour.IsClosed = false;
                    _operatingHourRepository.Update(existingHour);
                    resultHours.Add(existingHour);
                }
                else
                {
                    var newHour = new VenueOperatingHour
                    {
                        HoursId = Guid.NewGuid(),
                        VenueId = request.VenueId,
                        DayOfWeek = dayOfWeek,
                        OpenTime = openTime,
                        CloseTime = closeTime,
                        IsClosed = false
                    };
                    newHoursToAdd.Add(newHour);
                    resultHours.Add(newHour);
                }
            }
        }

        // Add all new hours at once
        if (newHoursToAdd.Count > 0)
        {
            await _operatingHourRepository.AddRangeAsync(newHoursToAdd, cancellationToken);
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        var response = new UpdateOperatingHoursResponse
        {
            VenueId = request.VenueId,
            OperatingHours = resultHours
                .OrderBy(h => (int)h.DayOfWeek)
                .Select(h => new OperatingHourResultDto(
                    h.HoursId,
                    (int)h.DayOfWeek,
                    h.DayOfWeek.ToString(),
                    h.OpenTime,
                    h.CloseTime,
                    h.IsClosed
                ))
                .ToList(),
            UpdatedAt = DateTime.UtcNow
        };

        return Result.Success(response);
    }
}
