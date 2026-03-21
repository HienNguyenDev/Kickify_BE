using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.Holidays.Commands.DeleteHoliday;

public record DeleteHolidayCommand(
    Guid HolidayId
) : ICommand<DeleteHolidayResponse>;
