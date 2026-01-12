using FluentValidation;

namespace Kickify.Application.Features.Venues.Commands.AddField
{
    public class AddFieldCommandValidator : AbstractValidator<AddFieldCommand>
    {
        public AddFieldCommandValidator()
        {
            RuleFor(x => x.VenueId)
                .NotEmpty().WithMessage("VenueId is required");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Name is required")
                .MaximumLength(100).WithMessage("Name must not exceed 100 characters");

            RuleFor(x => x.FieldType)
                .NotEmpty().WithMessage("FieldType is required");

            RuleFor(x => x.MaxPlayers)
                .GreaterThan(0).WithMessage("MaxPlayers must be greater than 0");

            RuleFor(x => x.PricePerHour)
                .GreaterThan(0).WithMessage("PricePerHour must be greater than 0");

            RuleFor(x => x.Description)
                .MaximumLength(1000).When(x => !string.IsNullOrEmpty(x.Description))
                .WithMessage("Description must not exceed 1000 characters");
        }
    }
}
