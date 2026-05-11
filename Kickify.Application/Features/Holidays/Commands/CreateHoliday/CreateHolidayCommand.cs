using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.Holidays.Commands.CreateHoliday;

public record CreateHolidayCommand(
    DateTime StartDate,
    DateTime EndDate,
    string Name
) : ICommand<CreateHolidayResponse>;
