using FluentValidation;
using Kickify.Domain.Enums;

namespace Kickify.Application.Features.Analytics.Queries.GetAdminBookingRevenueList;

public class GetAdminBookingRevenueListQueryValidator : AbstractValidator<GetAdminBookingRevenueListQuery>
{
    private static readonly DateTime MinReportDate = new(1900, 1, 1, 0, 0, 0, DateTimeKind.Unspecified);
    private static readonly DateTime MaxReportDate = new(2100, 12, 31, 0, 0, 0, DateTimeKind.Unspecified);

    private static readonly TransactionType[] ValidSources =
    [
        TransactionType.BookingCommission,
        TransactionType.WithdrawalFee,
        TransactionType.PremiumPurchase
    ];

    public GetAdminBookingRevenueListQueryValidator()
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

        RuleFor(x => x.FeeSource)
            .Must(s => s is null || ValidSources.Contains(s.Value))
            .WithMessage("feeSource must be one of: BookingCommission, WithdrawalFee, PremiumPurchase.");

        RuleFor(x => x.Page).GreaterThanOrEqualTo(1);
        RuleFor(x => x.PageSize).InclusiveBetween(1, 200);
    }
}
