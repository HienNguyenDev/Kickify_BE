using Kickify.Domain.Entities;
using Kickify.Domain.Enums;

namespace Kickify.Application.Common.Pricing;

public static class MatchPriceCalculator
{
    public static MatchPriceResult CalculateMatchPrice(
        Field field,
        DateTime matchDate,
        TimeSpan startTime,
        int durationMinutes,
        Holiday? holiday)
    {
        decimal totalSurcharge = 0m;
        decimal daySurcharge = 0m;
        decimal peakSurcharge = 0m;

        var isHolidayApplied = false;
        var isWeekendApplied = false;
        var isPeakApplied = false;

        // Priority 1: Holiday overrides weekend when holiday is not ignored by venue.
        var isIgnoredHoliday = holiday != null && field.Venue.IgnoredHolidays.Any(h => h.Id == holiday.Id);
        if (holiday != null && !isIgnoredHoliday)
        {
            var holidayFee = CalculateFee(field.HourlyRate, field.HolidaySurcharge, field.IsHolidaySurchargePercentage);
            totalSurcharge += holidayFee;
            daySurcharge += holidayFee;
            isHolidayApplied = true;
        }
        // Priority 2: Weekend surcharge when holiday surcharge was not applied.
        else if (matchDate.DayOfWeek is DayOfWeek.Saturday or DayOfWeek.Sunday)
        {
            var weekendFee = CalculateFee(field.HourlyRate, field.WeekendSurcharge, field.IsWeekendSurchargePercentage);
            totalSurcharge += weekendFee;
            daySurcharge += weekendFee;
            isWeekendApplied = true;
        }

        // Peak-hour surcharge is independent and only applies on configured peak days.
        var isPeakDay = field.PeakDaysOfWeek?.Contains((DayOfWeekEnum)matchDate.DayOfWeek) == true;
        var isInsidePeakWindow =
            field.PeakStartTime.HasValue &&
            field.PeakEndTime.HasValue &&
            startTime >= field.PeakStartTime.Value &&
            startTime < field.PeakEndTime.Value;

        if (isPeakDay && isInsidePeakWindow)
        {
            var peakFee = CalculateFee(field.HourlyRate, field.PeakHourSurcharge, field.IsPeakHourSurchargePercentage);
            totalSurcharge += peakFee;
            peakSurcharge += peakFee;
            isPeakApplied = true;
        }

        var durationHours = (decimal)durationMinutes / 60m;
        var totalPrice = (field.HourlyRate + totalSurcharge) * durationHours;

        return new MatchPriceResult(
            field.HourlyRate,
            daySurcharge,
            peakSurcharge,
            totalSurcharge,
            totalPrice,
            isHolidayApplied,
            isWeekendApplied,
            isPeakApplied,
            holiday?.Id,
            holiday?.Name);
    }

    private static decimal CalculateFee(decimal baseRate, decimal surcharge, bool isPercentage)
    {
        return isPercentage ? baseRate * (surcharge / 100m) : surcharge;
    }
}

public sealed record MatchPriceResult(
    decimal BaseHourlyRate,
    decimal DaySurcharge,
    decimal PeakSurcharge,
    decimal TotalSurcharge,
    decimal TotalPrice,
    bool IsHolidayApplied,
    bool IsWeekendApplied,
    bool IsPeakApplied,
    Guid? AppliedHolidayId,
    string? AppliedHolidayName);