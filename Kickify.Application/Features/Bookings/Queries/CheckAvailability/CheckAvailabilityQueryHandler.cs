using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;
using MediatR;

namespace Kickify.Application.Features.Bookings.Queries.CheckAvailability
{
    public class CheckAvailabilityQueryHandler : IRequestHandler<CheckAvailabilityQuery, Result<CheckAvailabilityResponse>>
    {
        private readonly IFieldRepository _fieldRepository;
        private readonly IBookingRepository _bookingRepository;

        public CheckAvailabilityQueryHandler(
            IFieldRepository fieldRepository,
            IBookingRepository bookingRepository)
        {
            _fieldRepository = fieldRepository;
            _bookingRepository = bookingRepository;
        }

        public async Task<Result<CheckAvailabilityResponse>> Handle(CheckAvailabilityQuery request, CancellationToken cancellationToken)
        {
            // Verify field exists and get venue with operating hours
            var field = await _fieldRepository.GetFieldWithVenueAsync(request.FieldId, cancellationToken);
            if (field == null)
            {
                return Result.Failure<CheckAvailabilityResponse>(FieldErrors.NotFound(request.FieldId));
            }

            // Get day of week
            var dayOfWeek = (DayOfWeekEnum)request.Date.DayOfWeek;

            // Get operating hours for this day
            var operatingHour = field.Venue.VenueOperatingHours
                .FirstOrDefault(oh => oh.DayOfWeek == dayOfWeek);

            if (operatingHour == null || operatingHour.IsClosed)
            {
                return Result.Success(new CheckAvailabilityResponse(
                    field.FieldId,
                    field.FieldName,
                    request.Date,
                    null,
                    null,
                    new List<TimeSlotDto>(),
                    "Venue is closed on this day"
                ));
            }

            // Get booked time slots for this field and date
            var bookedSlots = await _bookingRepository.GetBookedTimeSlotsAsync(
                request.FieldId,
                request.Date,
                cancellationToken);

            // Generate available time slots (1-hour intervals)
            var availableSlots = new List<TimeSlotDto>();
            var currentTime = operatingHour.OpenTime ?? TimeSpan.Zero;
            var closeTime = operatingHour.CloseTime ?? TimeSpan.Zero;

            while (currentTime.Add(TimeSpan.FromHours(1)) <= closeTime)
            {
                var endTime = currentTime.Add(TimeSpan.FromHours(1));

                // Check if this slot overlaps with any booked slot
                bool isBooked = bookedSlots.Any(booked =>
                    currentTime < booked.Item2 && endTime > booked.Item1);

                availableSlots.Add(new TimeSlotDto(
                    currentTime,
                    endTime,
                    !isBooked,
                    field.HourlyRate
                ));

                currentTime = endTime;
            }

            return Result.Success(new CheckAvailabilityResponse(
                field.FieldId,
                field.FieldName,
                request.Date,
                operatingHour.OpenTime,
                operatingHour.CloseTime,
                availableSlots,
                null
            ));
        }
    }
}
