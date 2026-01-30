using FluentValidation;

namespace Kickify.Application.Features.Fields.Commands.BlockFieldSlot
{
    public class BlockFieldSlotCommandValidator : AbstractValidator<BlockFieldSlotCommand>
    {
        public BlockFieldSlotCommandValidator()
        {
            RuleFor(x => x.VenueId)
                .NotEmpty()
                .WithMessage("VenueId is required");

            RuleFor(x => x.FieldId)
                .NotEmpty()
                .WithMessage("FieldId is required");

            RuleFor(x => x.Date)
                .NotEmpty()
                .WithMessage("Date is required")
                .GreaterThanOrEqualTo(DateTime.UtcNow.Date)
                .WithMessage("Date cannot be in the past");

            RuleFor(x => x.StartTime)
                .NotEmpty()
                .WithMessage("StartTime is required");

            RuleFor(x => x.EndTime)
                .NotEmpty()
                .WithMessage("EndTime is required")
                .GreaterThan(x => x.StartTime)
                .WithMessage("EndTime must be greater than StartTime");

            RuleFor(x => x.Reason)
                .NotEmpty()
                .WithMessage("Reason is required")
                .MaximumLength(500)
                .WithMessage("Reason cannot exceed 500 characters");

            RuleFor(x => x.Amount)
                .GreaterThanOrEqualTo(0)
                .WithMessage("Amount cannot be negative");
        }
    }
}
