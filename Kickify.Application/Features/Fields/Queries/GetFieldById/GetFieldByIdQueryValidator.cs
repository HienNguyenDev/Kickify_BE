using FluentValidation;

namespace Kickify.Application.Features.Fields.Queries.GetFieldById
{
    public class GetFieldByIdQueryValidator : AbstractValidator<GetFieldByIdQuery>
    {
        public GetFieldByIdQueryValidator()
        {
            RuleFor(x => x.FieldId)
                .NotEmpty()
                .WithMessage("FieldId is required");
        }
    }
}
