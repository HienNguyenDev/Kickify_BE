namespace Kickify.Application.Features.Holidays.Commands.CreateHoliday;

public record CreateHolidayResponse(
    Guid Id,
    DateTime Date,
    string Name,
    DateTime CreatedAt
);
