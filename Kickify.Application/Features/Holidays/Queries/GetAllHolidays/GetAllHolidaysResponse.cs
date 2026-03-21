namespace Kickify.Application.Features.Holidays.Queries.GetAllHolidays;

public record GetAllHolidaysResponse(List<HolidayItemDto> Holidays);

public record HolidayItemDto(
    Guid Id,
    string Name,
    DateTime Date
);