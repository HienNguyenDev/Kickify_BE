using FluentValidation;

namespace Kickify.Application.Features.Users.Queries.GetAllUsers
{
    public class GetAllUsersQueryValidator : AbstractValidator<GetAllUsersQuery>
    {
        public GetAllUsersQueryValidator()
        {
            RuleFor(x => x.Page)
                .GreaterThan(0).WithMessage("Page must be greater than 0");

            RuleFor(x => x.PageSize)
                .GreaterThan(0).WithMessage("Page size must be greater than 0")
                .LessThanOrEqualTo(100).WithMessage("Page size must not exceed 100");

            RuleFor(x => x.Role)
                .IsInEnum().WithMessage("Invalid user role")
                .When(x => x.Role.HasValue);

            RuleFor(x => x.SearchTerm)
                .MaximumLength(255).WithMessage("Search term must not exceed 255 characters")
                .When(x => !string.IsNullOrEmpty(x.SearchTerm));
        }
    }
}
