namespace Kickify.Application.Features.Holidays.Commands.UpdateHoliday;

public record UpdateHolidayResponse(
    Guid Id,
    DateTime Date,
    string Name,
    DateTime UpdatedAt
);
