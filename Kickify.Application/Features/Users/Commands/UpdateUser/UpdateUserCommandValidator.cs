using FluentValidation;

namespace Kickify.Application.Features.Users.Commands.UpdateUser
{
    public class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
    {
        public UpdateUserCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required");

            RuleFor(x => x.FullName)
                .MaximumLength(255).WithMessage("Full name must not exceed 255 characters")
                .When(x => !string.IsNullOrEmpty(x.FullName));

            RuleFor(x => x.Phone)
                .Matches(@"^\+?[1-9]\d{1,14}$").WithMessage("Invalid phone number format")
                .When(x => !string.IsNullOrEmpty(x.Phone));

            RuleFor(x => x.AvatarUrl)
                .Must(BeAValidUrl).WithMessage("Invalid avatar URL")
                .When(x => !string.IsNullOrEmpty(x.AvatarUrl));

            RuleFor(x => x.Bio)
                .MaximumLength(500).WithMessage("Bio must not exceed 500 characters")
                .When(x => !string.IsNullOrEmpty(x.Bio));

            RuleFor(x => x.DateOfBirth)
                .LessThan(DateTime.UtcNow).WithMessage("Date of birth must be in the past")
                .GreaterThan(DateTime.UtcNow.AddYears(-120)).WithMessage("Date of birth is invalid")
                .When(x => x.DateOfBirth.HasValue);

            RuleFor(x => x.Gender)
                .IsInEnum().WithMessage("Invalid gender")
                .When(x => x.Gender.HasValue);
        }

        private bool BeAValidUrl(string? url)
        {
            if (string.IsNullOrEmpty(url))
                return true;

            return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
        }
    }
}
