namespace Kickify.Application.Features.Holidays.Commands.CreateHoliday;

public record HolidayDto(
    Guid Id,
    DateTime Date,
    string Name
);

public record CreateHolidayResponse(
    List<HolidayDto> Holidays
);
