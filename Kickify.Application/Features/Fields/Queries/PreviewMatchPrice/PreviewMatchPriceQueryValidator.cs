using FluentValidation;

namespace Kickify.Application.Features.Fields.Queries.PreviewMatchPrice;

public sealed class PreviewMatchPriceQueryValidator : AbstractValidator<PreviewMatchPriceQuery>
{
    public PreviewMatchPriceQueryValidator()
    {
        RuleFor(x => x.FieldId)
            .NotEmpty()
            .WithMessage("FieldId is required");

        RuleFor(x => x.MatchDate)
            .GreaterThanOrEqualTo(DateTime.UtcNow.Date)
            .WithMessage("MatchDate cannot be in the past");

        RuleFor(x => x.StartTime)
            .NotEmpty()
            .WithMessage("StartTime is required");

        RuleFor(x => x.DurationMinutes)
            .Must(d => d == 60 || d == 90 || d == 120)
            .WithMessage("Duration must be 60, 90, or 120 minutes");
    }
}
