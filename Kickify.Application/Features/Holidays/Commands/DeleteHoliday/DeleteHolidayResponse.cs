namespace Kickify.Application.Features.Holidays.Commands.DeleteHoliday;

public record DeleteHolidayResponse(
    Guid HolidayId,
    bool IsDeleted
);
