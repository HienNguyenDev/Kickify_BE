using FluentValidation;
using Kickify.Domain.Enums;

namespace Kickify.Application.Features.ContentReports.Commands.ReportContent;

public class ReportContentCommandValidator : AbstractValidator<ReportContentCommand>
{
    public ReportContentCommandValidator()
    {
        RuleFor(x => x.ContentId)
            .NotEmpty();

        RuleFor(x => x.Description)
            .NotEmpty()
            .WithMessage("Description is required when reason is 'Other'.")
            .When(x => x.Reason == ContentReportReason.Other);
    }
}
