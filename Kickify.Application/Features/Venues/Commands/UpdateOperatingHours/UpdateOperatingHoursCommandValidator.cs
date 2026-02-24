using FluentValidation;

namespace Kickify.Application.Features.Venues.Commands.UpdateOperatingHours;

public class UpdateOperatingHoursCommandValidator : AbstractValidator<UpdateOperatingHoursCommand>
{
    public UpdateOperatingHoursCommandValidator()
    {
        RuleFor(x => x.VenueId)
            .NotEmpty()
            .WithMessage("VenueId is required");

        RuleFor(x => x.OperatingHours)
            .NotNull()
            .WithMessage("OperatingHours is required")
            .Must(x => x.Count == 7)
            .WithMessage("OperatingHours must contain exactly 7 items (one for each day of the week)");

        RuleForEach(x => x.OperatingHours)
            .ChildRules(item =>
            {
                item.RuleFor(x => x.DayOfWeek)
                    .InclusiveBetween(0, 6)
                    .WithMessage("DayOfWeek must be between 0 (Sunday) and 6 (Saturday)");

                item.RuleFor(x => x.OpenTime)
                    .Must(BeValidTimeOrNullOrEmpty)
                    .WithMessage("OpenTime must be a valid time format (HH:mm) or null/empty");

                item.RuleFor(x => x.CloseTime)
                    .Must(BeValidTimeOrNullOrEmpty)
                    .WithMessage("CloseTime must be a valid time format (HH:mm) or null/empty");
            });

        RuleFor(x => x.OperatingHours)
            .Must(HaveUniqueDays)
            .WithMessage("Each day of the week must appear exactly once");
    }

    private static bool BeValidTimeOrNullOrEmpty(string? time)
    {
        if (time == null) return true;
        if (time == string.Empty) return true;
        return TimeSpan.TryParse(time, out _);
    }

    private static bool HaveUniqueDays(List<OperatingHourItemDto> items)
    {
        if (items == null || items.Count != 7) return false;
        var days = items.Select(x => x.DayOfWeek).Distinct().ToList();
        return days.Count == 7 && days.All(d => d >= 0 && d <= 6);
    }
}
