using AutoMapper;
using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Persistence;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Entities;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.Venues.Commands.UpdateVenue
{
    public class UpdateVenueCommandHandler : ICommandHandler<UpdateVenueCommand, UpdateVenueResponse>
    {
        private readonly IVenueRepository _venueRepository;
        private readonly IHolidayRepository _holidayRepository;
        private readonly IVenueOperatingHourRepository _operatingHourRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IUserContext _userContext;

        public UpdateVenueCommandHandler(
            IVenueRepository venueRepository,
            IHolidayRepository holidayRepository,
            IVenueOperatingHourRepository operatingHourRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper,
            IUserContext userContext)
        {
            _venueRepository = venueRepository;
            _holidayRepository = holidayRepository;
            _operatingHourRepository = operatingHourRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userContext = userContext;
        }

        public async Task<Result<UpdateVenueResponse>> Handle(UpdateVenueCommand request, CancellationToken cancellationToken)
        {
            var userId = _userContext.UserId;
            
            // Get venue with tracking for update
            var venue = await _venueRepository.GetVenueForUpdateAsync(request.VenueId, cancellationToken);

            if (venue == null)
            {
                return Result.Failure<UpdateVenueResponse>(VenueErrors.NotFound(request.VenueId));
            }

            // Check if user is the owner
            if (venue.OwnerId != userId)
            {
                return Result.Failure<UpdateVenueResponse>(VenueErrors.Unauthorized);
            }

            // Map properties from command to entity
            // Rule: null = keep old value, non-null (including empty string) = update
            _mapper.Map(request, venue);

            if (request.IgnoredHolidayIds != null)
            {
                var ignoredHolidayIds = request.IgnoredHolidayIds.Distinct().ToList();
                var holidays = await _holidayRepository.GetByIdsAsync(ignoredHolidayIds, cancellationToken);

                if (holidays.Count != ignoredHolidayIds.Count)
                {
                    var missingHolidayIds = ignoredHolidayIds.Except(holidays.Select(h => h.Id)).ToList();
                    return Result.Failure<UpdateVenueResponse>(HolidayErrors.InvalidIds(missingHolidayIds));
                }

                await _venueRepository.SyncIgnoredHolidaysAsync(venue, holidays, cancellationToken);
            }

            venue.UpdatedAt = DateTime.UtcNow;

            _venueRepository.Update(venue);

            // Update operating hours if provided
            var resultHours = new List<VenueOperatingHour>();
            if (request.OperatingHours != null && request.OperatingHours.Count > 0)
            {
                resultHours = await UpdateOperatingHoursAsync(request.VenueId, request.OperatingHours, cancellationToken);

                // Merge with all existing hours from DB so we have a complete picture of ALL days,
                // not just the days the frontend sent. This prevents accidentally removing peak hours
                // for days that weren't part of this update request.
                var allExistingHours = await _operatingHourRepository.GetByVenueIdAsync(request.VenueId, cancellationToken);
                var coveredDays = resultHours.Select(h => h.DayOfWeek).ToHashSet();
                foreach (var existingHour in allExistingHours)
                {
                    if (!coveredDays.Contains(existingHour.DayOfWeek))
                    {
                        resultHours.Add(existingHour);
                    }
                }

                // TASK 3: Clean up orphaned peak hours for closed days AND shrunk operating hours
                foreach (var childField in venue.Fields)
                {
                    var peakHoursToRemove = new List<FieldPeakHour>();
                    var isFieldUpdated = false;

                    foreach (var peakHour in childField.PeakHours)
                    {
                        var daysToRemove = new List<DayOfWeekEnum>();

                        foreach (var day in peakHour.ApplicableDays)
                        {
                            var operatingHour = resultHours.FirstOrDefault(h => h.DayOfWeek == day);

                            if (operatingHour == null || operatingHour.IsClosed)
                            {
                                daysToRemove.Add(day);
                            }
                            else if (operatingHour.OpenTime == null || operatingHour.CloseTime == null)
                            {
                                daysToRemove.Add(day);
                            }
                            else if (peakHour.StartTime < operatingHour.OpenTime.Value ||
                                     peakHour.EndTime > operatingHour.CloseTime.Value)
                            {
                                daysToRemove.Add(day);
                            }
                        }

                        if (daysToRemove.Count > 0)
                        {
                            // Reassign with a NEW list instance to ensure EF Core's ValueComparer
                            // detects the change on the PostgreSQL integer[] column.
                            peakHour.ApplicableDays = peakHour.ApplicableDays
                                .Where(d => !daysToRemove.Contains(d))
                                .ToList();
                            isFieldUpdated = true;

                            if (peakHour.ApplicableDays.Count == 0)
                            {
                                peakHoursToRemove.Add(peakHour);
                            }
                        }
                    }

                    foreach (var peakHour in peakHoursToRemove)
                    {
                        childField.PeakHours.Remove(peakHour);
                    }

                    if (isFieldUpdated)
                    {
                        childField.UpdatedAt = DateTime.UtcNow;
                    }
                }
            }
            else
            {
                // Get existing hours to return in response
                resultHours = (await _operatingHourRepository.GetByVenueIdAsync(request.VenueId, cancellationToken)).ToList();
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success(new UpdateVenueResponse(
                venue.VenueId,
                venue.VenueName,
                venue.Address,
                venue.Latitude,
                venue.Longitude,
                venue.ContactPhone,
                venue.ContactEmail,
                venue.Description,
                venue.Amenities,
                resultHours.OrderBy(h => h.DayOfWeek).Select(h => new UpdateVenueOperatingHourResponseDto(
                    h.DayOfWeek.ToString(),
                    h.OpenTime,
                    h.CloseTime,
                    h.IsClosed
                )).ToList(),
                venue.UpdatedAt
            ));
        }

        private async Task<List<VenueOperatingHour>> UpdateOperatingHoursAsync(
            Guid venueId,
            List<UpdateVenueOperatingHourItemDto> operatingHours,
            CancellationToken cancellationToken)
        {
            var existingHours = await _operatingHourRepository.GetByVenueIdAsync(venueId, cancellationToken);
            var resultHours = new List<VenueOperatingHour>();
            var newHoursToAdd = new List<VenueOperatingHour>();

            foreach (var item in operatingHours)
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
                            VenueId = venueId,
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
                            VenueId = venueId,
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
                            VenueId = venueId,
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

            // Add new hours
            if (newHoursToAdd.Count > 0)
            {
                await _operatingHourRepository.AddRangeAsync(newHoursToAdd);
            }

            return resultHours;
        }
    }
}
