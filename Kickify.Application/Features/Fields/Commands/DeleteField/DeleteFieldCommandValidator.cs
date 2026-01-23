using FluentValidation;

namespace Kickify.Application.Features.Fields.Commands.DeleteField
{
    public class DeleteFieldCommandValidator : AbstractValidator<DeleteFieldCommand>
    {
        public DeleteFieldCommandValidator()
        {
            RuleFor(x => x.FieldId)
                .NotEmpty()
                .WithMessage("FieldId is required");

            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("UserId is required");
        }
    }
}
