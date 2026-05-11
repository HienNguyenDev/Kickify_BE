using FluentValidation;

namespace Kickify.Application.Features.Holidays.Commands.CreateHoliday;

public class CreateHolidayCommandValidator : AbstractValidator<CreateHolidayCommand>
{
    public CreateHolidayCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200).WithMessage("Name must not exceed 200 characters");

        RuleFor(x => x.StartDate)
            .NotEqual(default(DateTime)).WithMessage("StartDate is required");

        RuleFor(x => x.EndDate)
            .NotEqual(default(DateTime)).WithMessage("EndDate is required");
    }
}
