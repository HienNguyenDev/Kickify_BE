using FluentValidation;

namespace Kickify.Application.Features.Holidays.Commands.DeleteHoliday;

public class DeleteHolidayCommandValidator : AbstractValidator<DeleteHolidayCommand>
{
    public DeleteHolidayCommandValidator()
    {
        RuleFor(x => x.HolidayId)
            .NotEmpty().WithMessage("HolidayId is required");
    }
}
