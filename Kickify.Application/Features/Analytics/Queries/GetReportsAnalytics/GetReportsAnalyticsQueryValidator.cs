using FluentValidation;

namespace Kickify.Application.Features.Analytics.Queries.GetReportsAnalytics;

public class GetReportsAnalyticsQueryValidator : AbstractValidator<GetReportsAnalyticsQuery>
{
    private static readonly DateTime MinReportDate = new(1900, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);
    private static readonly DateTime MaxReportDate = new(2100, 12, 31, 0, 0, 0, DateTimeKind.Unspecified);

    public GetReportsAnalyticsQueryValidator()
    {
        RuleFor(x => x.FromDate)
            .Must(d => d.Date >= MinReportDate && d.Date <= MaxReportDate)
            .WithMessage($"FromDate must be between {MinReportDate:yyyy-MM-dd} and {MaxReportDate:yyyy-MM-dd}.");

        RuleFor(x => x.ToDate)
            .Must(d => d.Date >= MinReportDate && d.Date <= MaxReportDate)
            .WithMessage($"ToDate must be between {MinReportDate:yyyy-MM-dd} and {MaxReportDate:yyyy-MM-dd}.");

        RuleFor(x => x)
            .Must(x => x.ToDate.Date >= x.FromDate.Date)
            .WithMessage("ToDate must be greater than or equal to FromDate.");
    }
}
