using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.Bookings.Queries.CheckAvailability
{
    public class CheckAvailabilityQueryHandler : IQueryHandler<CheckAvailabilityQuery, CheckAvailabilityResponse>
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

            // Check if venue is archived
            if (field.Venue.Status == VenueStatus.Archived)
            {
                return Result.Failure<CheckAvailabilityResponse>(FieldErrors.VenueArchived);
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

            // Generate available time slots (30-minute intervals)
            var availableSlots = new List<TimeSlotDto>();
            var currentSlotTime = operatingHour.OpenTime ?? TimeSpan.Zero;
            var closeTime = operatingHour.CloseTime ?? TimeSpan.Zero;

            var utcNow = DateTime.UtcNow;

            TimeZoneInfo vnTimeZone;
            try
            {
                vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"); // Windows
            }
            catch (TimeZoneNotFoundException)
            {
                vnTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh"); // Linux/Docker
            }

            var nowInVietnam = TimeZoneInfo.ConvertTimeFromUtc(utcNow, vnTimeZone);

            var todayInVietnam = nowInVietnam.Date;
            var currentTimeOfDayInVietnam = nowInVietnam.TimeOfDay;

            var isToday = request.Date.Date == todayInVietnam;
            var isPastDate = request.Date.Date < todayInVietnam;

            // ====================================================================================
            // LOGIC M?I: KI?M TRA OVERLAP D?A TRÊN TH?I LÝ?NG T?I THI?U 60 PHÚT
            // ====================================================================================
            var minMatchDuration = TimeSpan.FromMinutes(60);

            // V?ng l?p ch? ch?y khi slot hi?n t?i + 60p v?n n?m trong gi? m? c?a
            while (currentSlotTime.Add(minMatchDuration) <= closeTime)
            {
                bool isAvailable = true;

                // Th?i ði?m k?t thúc gi? ð?nh n?u ð?t slot này
                var expectedEndTime = currentSlotTime.Add(minMatchDuration);

                if (isPastDate)
                {
                    isAvailable = false;
                }
                else if (isToday && currentSlotTime < currentTimeOfDayInVietnam)
                {
                    isAvailable = false;
                }
                else
                {
                    // Check ð?ng l?ch (Overlap) cho toàn b? kho?ng th?i gian 60 phút
                    isAvailable = !bookedSlots.Any(booked =>
                        currentSlotTime < booked.Item2 && expectedEndTime > booked.Item1);
                }

                availableSlots.Add(new TimeSlotDto(
                    currentSlotTime,
                    isAvailable,
                    field.HourlyRate
                ));

                // Chuy?n sang slot ti?p theo (cách 30 phút)
                currentSlotTime = currentSlotTime.Add(TimeSpan.FromMinutes(30));
            }
            // ====================================================================================

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