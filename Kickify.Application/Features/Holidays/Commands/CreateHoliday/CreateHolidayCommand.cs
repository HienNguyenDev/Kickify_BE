using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.Holidays.Commands.CreateHoliday;

public record CreateHolidayCommand(
    DateTime Date,
    string Name
) : ICommand<CreateHolidayResponse>;
