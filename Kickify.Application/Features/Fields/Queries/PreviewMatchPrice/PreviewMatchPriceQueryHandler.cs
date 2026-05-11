using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Common.Pricing;
using Kickify.Domain.Common;
using Kickify.Domain.Enums;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.Fields.Queries.PreviewMatchPrice;

public sealed class PreviewMatchPriceQueryHandler : IQueryHandler<PreviewMatchPriceQuery, PreviewMatchPriceResponse>
{
    private readonly IFieldRepository _fieldRepository;
    private readonly IBookingRepository _bookingRepository;
    private readonly IHolidayRepository _holidayRepository;

    public PreviewMatchPriceQueryHandler(
        IFieldRepository fieldRepository,
        IBookingRepository bookingRepository,
        IHolidayRepository holidayRepository)
    {
        _fieldRepository = fieldRepository;
        _bookingRepository = bookingRepository;
        _holidayRepository = holidayRepository;
    }

    public async Task<Result<PreviewMatchPriceResponse>> Handle(PreviewMatchPriceQuery request, CancellationToken cancellationToken)
    {
        var field = await _fieldRepository.GetFieldWithVenueAsync(request.FieldId, cancellationToken);
        if (field == null)
        {
            return Result.Failure<PreviewMatchPriceResponse>(FieldErrors.NotFound(request.FieldId));
        }

        var endTime = request.StartTime.Add(TimeSpan.FromMinutes(request.DurationMinutes));
        var dayOfWeek = (DayOfWeekEnum)request.MatchDate.DayOfWeek;

        var operatingHour = field.Venue.VenueOperatingHours
            .FirstOrDefault(oh => oh.DayOfWeek == dayOfWeek);

        var isVenueClosed = operatingHour == null || operatingHour.IsClosed;
        if (isVenueClosed)
        {
            return Result.Success(BuildUnavailableResponse(
                field,
                request,
                endTime,
                isTimeSlotAvailable: false,
                isWithinOperatingHours: false,
                isVenueClosed: true,
                message: $"Venue is closed on {request.MatchDate:dddd}"));
        }

        var openTime = operatingHour!.OpenTime ?? TimeSpan.Zero;
        var closeTime = operatingHour.CloseTime ?? TimeSpan.Zero;
        var isWithinOperatingHours = request.StartTime >= openTime && endTime <= closeTime;

        if (!isWithinOperatingHours)
        {
            return Result.Success(BuildUnavailableResponse(
                field,
                request,
                endTime,
                isTimeSlotAvailable: false,
                isWithinOperatingHours: false,
                isVenueClosed: false,
                message: $"Requested time ({request.StartTime:hh\\:mm} - {endTime:hh\\:mm}) is outside operating hours ({openTime:hh\\:mm} - {closeTime:hh\\:mm})"));
        }

        var isSlotAvailable = await _bookingRepository.IsTimeSlotAvailableAsync(
            request.FieldId,
            request.MatchDate,
            request.StartTime,
            endTime,
            cancellationToken);

        var holiday = await _holidayRepository.GetByDateAsync(request.MatchDate, cancellationToken);
        var priceResult = MatchPriceCalculator.CalculateMatchPrice(
            field,
            request.MatchDate,
            request.StartTime,
            request.DurationMinutes,
            holiday);

        var totalSlots = CalculateTotalSlots(field.FieldType);
        var depositPerPerson = totalSlots == 0 ? 0 : Math.Round(priceResult.TotalPrice / totalSlots, 0);

        return Result.Success(new PreviewMatchPriceResponse(
            field.FieldId,
            field.FieldName,
            field.VenueId,
            field.Venue.VenueName,
            request.MatchDate,
            request.StartTime,
            endTime,
            request.DurationMinutes,
            isSlotAvailable,
            isSlotAvailable ? "Price preview calculated successfully" : "Requested slot is already booked",
            priceResult.BaseHourlyRate,
            priceResult.DaySurcharge,
            priceResult.PeakSurcharge,
            priceResult.TotalSurcharge,
            priceResult.TotalPrice,
            totalSlots,
            depositPerPerson,
            priceResult.IsHolidayApplied,
            priceResult.IsWeekendApplied,
            priceResult.IsPeakApplied,
            priceResult.AppliedHolidayId,
            priceResult.AppliedHolidayName,
            isSlotAvailable,
            true,
            false));
    }

    private static PreviewMatchPriceResponse BuildUnavailableResponse(
        Kickify.Domain.Entities.Field field,
        PreviewMatchPriceQuery request,
        TimeSpan endTime,
        bool isTimeSlotAvailable,
        bool isWithinOperatingHours,
        bool isVenueClosed,
        string message)
    {
        return new PreviewMatchPriceResponse(
            field.FieldId,
            field.FieldName,
            field.VenueId,
            field.Venue.VenueName,
            request.MatchDate,
            request.StartTime,
            endTime,
            request.DurationMinutes,
            false,
            message,
            field.HourlyRate,
            0,
            0,
            0,
            0,
            CalculateTotalSlots(field.FieldType),
            0,
            false,
            false,
            false,
            null,
            null,
            isTimeSlotAvailable,
            isWithinOperatingHours,
            isVenueClosed);
    }

    private static int CalculateTotalSlots(FieldType format)
    {
        return format switch
        {
            FieldType.FiveVsFive => 10,
            FieldType.SevenVsSeven => 14,
            FieldType.ElevenVsEleven => 22,
            _ => 0
        };
    }
}
