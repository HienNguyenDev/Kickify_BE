using FluentValidation;

namespace Kickify.Application.Features.Holidays.Commands.UpdateHoliday;

public class UpdateHolidayCommandValidator : AbstractValidator<UpdateHolidayCommand>
{
    public UpdateHolidayCommandValidator()
    {
        RuleFor(x => x.HolidayId)
            .NotEmpty().WithMessage("HolidayId is required");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters");

        RuleFor(x => x.Date)
            .NotEqual(default(DateTime)).WithMessage("Date is required");
    }
}
