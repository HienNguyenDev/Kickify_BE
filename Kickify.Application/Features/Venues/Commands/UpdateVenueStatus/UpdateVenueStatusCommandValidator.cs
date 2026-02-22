using FluentValidation;
using Kickify.Domain.Enums;

namespace Kickify.Application.Features.Venues.Commands.UpdateVenueStatus;

public class UpdateVenueStatusCommandValidator : AbstractValidator<UpdateVenueStatusCommand>
{
    public UpdateVenueStatusCommandValidator()
    {
        RuleFor(x => x.VenueId)
            .NotEmpty().WithMessage("VenueId is required");

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required")
            .Must(BeValidVenueStatus)
            .WithMessage("Invalid status. Allowed values: Pending, Active, Approved, Rejected, Suspended");

        RuleFor(x => x.AdminNotes)
            .MaximumLength(1000).WithMessage("AdminNotes must not exceed 1000 characters");
    }

    private static bool BeValidVenueStatus(string status)
    {
        return Enum.TryParse<VenueStatus>(status, true, out _);
    }
}
