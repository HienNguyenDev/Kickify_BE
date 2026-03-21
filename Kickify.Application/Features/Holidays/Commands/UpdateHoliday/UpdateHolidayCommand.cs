using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.Holidays.Commands.UpdateHoliday;

public record UpdateHolidayCommand(
    Guid HolidayId,
    DateTime Date,
    string Name
) : ICommand<UpdateHolidayResponse>;
