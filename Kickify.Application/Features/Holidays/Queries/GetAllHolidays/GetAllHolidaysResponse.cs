namespace Kickify.Application.Features.Holidays.Queries.GetAllHolidays;

public record GetAllHolidaysResponse(
    List<HolidayItemDto> Items,
    int TotalCount,
    int Page,
    int PageSize,
    int TotalPages
);

public record HolidayItemDto(
    Guid Id,
    string Name,
    DateTime Date
);