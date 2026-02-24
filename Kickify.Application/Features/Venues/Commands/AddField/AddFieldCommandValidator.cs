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

            

            RuleFor(x => x.HourlyRate)
                .GreaterThan(0).WithMessage("HourlyRate must be greater than 0");

            
        }
    }
}
