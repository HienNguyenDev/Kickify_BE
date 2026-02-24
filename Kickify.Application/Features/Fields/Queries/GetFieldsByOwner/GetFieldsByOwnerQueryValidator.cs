using FluentValidation;

namespace Kickify.Application.Features.Fields.Queries.GetFieldsByOwner
{
    public class GetFieldsByOwnerQueryValidator : AbstractValidator<GetFieldsByOwnerQuery>
    {
        public GetFieldsByOwnerQueryValidator()
        {
            RuleFor(x => x.OwnerId)
                .NotEmpty()
                .WithMessage("OwnerId is required");

            RuleFor(x => x.Page)
                .GreaterThan(0)
                .WithMessage("Page must be greater than 0");

            RuleFor(x => x.PageSize)
                .GreaterThan(0)
                .LessThanOrEqualTo(100)
                .WithMessage("PageSize must be between 1 and 100");
        }
    }
}
