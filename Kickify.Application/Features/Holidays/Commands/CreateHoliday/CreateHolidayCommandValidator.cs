using FluentValidation;

namespace Kickify.Application.Features.Holidays.Commands.CreateHoliday;

public class CreateHolidayCommandValidator : AbstractValidator<CreateHolidayCommand>
{
    public CreateHolidayCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters");

        RuleFor(x => x.Date)
            .NotEqual(default(DateTime)).WithMessage("Date is required");
    }
}
