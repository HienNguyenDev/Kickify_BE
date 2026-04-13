using FluentValidation;

namespace Kickify.Application.Features.SystemLogs.Queries.GetSystemLogs;

public class GetSystemLogsQueryValidator : AbstractValidator<GetSystemLogsQuery>
{
    private static readonly DateTime MinDate = new(2020, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);
    private static readonly DateTime MaxDate = new(2100, 12, 31, 0, 0, 0, DateTimeKind.Unspecified);

    public GetSystemLogsQueryValidator()
    {
        RuleFor(x => x.FromDate)
            .Must(d => d.Date >= MinDate && d.Date <= MaxDate)
            .WithMessage($"FromDate must be between {MinDate:yyyy-MM-dd} and {MaxDate:yyyy-MM-dd}.");

        RuleFor(x => x.ToDate)
            .Must(d => d.Date >= MinDate && d.Date <= MaxDate)
            .WithMessage($"ToDate must be between {MinDate:yyyy-MM-dd} and {MaxDate:yyyy-MM-dd}.");

        RuleFor(x => x)
            .Must(x => x.ToDate.Date >= x.FromDate.Date)
            .WithMessage("ToDate must be greater than or equal to FromDate.");

        RuleFor(x => x)
            .Must(x => (x.ToDate.Date - x.FromDate.Date).TotalDays <= 90)
            .WithMessage("Date range must not exceed 90 days.");

        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 200);
    }
}
