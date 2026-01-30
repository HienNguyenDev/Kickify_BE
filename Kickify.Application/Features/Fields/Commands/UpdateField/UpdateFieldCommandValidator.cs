using FluentValidation;

namespace Kickify.Application.Features.Fields.Commands.UpdateField
{
    public class UpdateFieldCommandValidator : AbstractValidator<UpdateFieldCommand>
    {
        public UpdateFieldCommandValidator()
        {
            RuleFor(x => x.FieldId)
                .NotEmpty()
                .WithMessage("FieldId is required");

            When(x => !string.IsNullOrEmpty(x.FieldName), () =>
            {
                RuleFor(x => x.FieldName)
                    .MaximumLength(100)
                    .WithMessage("FieldName must not exceed 100 characters");
            });

            When(x => x.HourlyRate.HasValue, () =>
            {
                RuleFor(x => x.HourlyRate)
                    .GreaterThan(0)
                    .WithMessage("HourlyRate must be greater than 0");
            });

            When(x => x.PeakHourSurcharge.HasValue, () =>
            {
                RuleFor(x => x.PeakHourSurcharge)
                    .GreaterThanOrEqualTo(0)
                    .WithMessage("PeakHourSurcharge must be greater than or equal to 0");
            });
        }
    }
}
