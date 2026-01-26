using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.Bookings.Queries.CheckConsecutiveSlots
{
    public class CheckConsecutiveSlotsQueryHandler : IQueryHandler<CheckConsecutiveSlotsQuery, CheckConsecutiveSlotsResponse>
    {
        private readonly IFieldRepository _fieldRepository;
        private readonly IBookingRepository _bookingRepository;

        public CheckConsecutiveSlotsQueryHandler(
            IFieldRepository fieldRepository,
            IBookingRepository bookingRepository)
        {
            _fieldRepository = fieldRepository;
            _bookingRepository = bookingRepository;
        }

        public async Task<Result<CheckConsecutiveSlotsResponse>> Handle(CheckConsecutiveSlotsQuery request, CancellationToken cancellationToken)
        {
            // Verify field exists and get venue with operating hours
            var field = await _fieldRepository.GetFieldWithVenueAsync(request.FieldId, cancellationToken);
            if (field == null)
            {
                return Result.Failure<CheckConsecutiveSlotsResponse>(FieldErrors.NotFound(request.FieldId));
            }

            // Calculate end time
            var endTime = request.StartTime.Add(TimeSpan.FromMinutes(request.DurationMinutes));

            // Get day of week
            var dayOfWeek = (DayOfWeekEnum)request.Date.DayOfWeek;

            // Get operating hours for this day
            var operatingHour = field.Venue.VenueOperatingHours
                .FirstOrDefault(oh => oh.DayOfWeek == dayOfWeek);

            if (operatingHour == null || operatingHour.IsClosed)
            {
                return Result.Success(new CheckConsecutiveSlotsResponse(
                    field.FieldId,
                    field.FieldName,
                    request.Date,
                    request.StartTime,
                    endTime,
                    request.DurationMinutes,
                    false,
                    new List<string>(),
                    "Venue is closed on this day",
                    field.HourlyRate,
                    0
                ));
            }

            // Check if requested time is within operating hours
            var openTime = operatingHour.OpenTime ?? TimeSpan.Zero;
            var closeTime = operatingHour.CloseTime ?? TimeSpan.Zero;

            if (request.StartTime < openTime || endTime > closeTime)
            {
                return Result.Success(new CheckConsecutiveSlotsResponse(
                    field.FieldId,
                    field.FieldName,
                    request.Date,
                    request.StartTime,
                    endTime,
                    request.DurationMinutes,
                    false,
                    new List<string>(),
                    $"Requested time is outside operating hours ({openTime:hh\\:mm} - {closeTime:hh\\:mm})",
                    field.HourlyRate,
                    0
                ));
            }

            // Get all booked slots for this field and date
            var bookedSlots = await _bookingRepository.GetBookedTimeSlotsAsync(
                request.FieldId,
                request.Date,
                cancellationToken);

            // Generate all 30-minute slots needed for this booking
            var requiredSlots = new List<TimeSpan>();
            var currentSlot = request.StartTime;
            while (currentSlot < endTime)
            {
                requiredSlots.Add(currentSlot);
                currentSlot = currentSlot.Add(TimeSpan.FromMinutes(30));
            }

            // Check which slots are unavailable
            var unavailableSlots = new List<string>();
            foreach (var slot in requiredSlots)
            {
                // A slot is unavailable if it falls within any existing booking
                bool isBooked = bookedSlots.Any(booked =>
                    slot >= booked.Item1 && slot < booked.Item2);

                if (isBooked)
                {
                    unavailableSlots.Add(slot.ToString(@"hh\:mm"));
                }
            }

            bool isAvailable = unavailableSlots.Count == 0;

            // Calculate cost
            decimal durationHours = (decimal)request.DurationMinutes / 60;
            decimal totalCost = field.HourlyRate * durationHours;

            string? message = null;
            if (!isAvailable)
            {
                message = $"Some slots are already booked: {string.Join(", ", unavailableSlots)}";
            }
            else
            {
                message = $"All {requiredSlots.Count} consecutive slots are available";
            }

            return Result.Success(new CheckConsecutiveSlotsResponse(
                field.FieldId,
                field.FieldName,
                request.Date,
                request.StartTime,
                endTime,
                request.DurationMinutes,
                isAvailable,
                unavailableSlots,
                message,
                field.HourlyRate,
                totalCost
            ));
        }
    }
}
