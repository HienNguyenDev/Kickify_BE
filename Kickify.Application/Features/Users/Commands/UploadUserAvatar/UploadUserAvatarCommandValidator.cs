using FluentValidation;

namespace Kickify.Application.Features.Users.Commands.UploadUserAvatar;

public class UploadUserAvatarCommandValidator : AbstractValidator<UploadUserAvatarCommand>
{
    private static readonly string[] AllowedContentTypes = { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
    private const long MaxFileSize = 5 * 1024 * 1024;

    public UploadUserAvatarCommandValidator()
    {
        RuleFor(x => x.File).NotNull().WithMessage("File is required");
        RuleFor(x => x.File.ContentType).Must(ct => AllowedContentTypes.Contains(ct)).WithMessage("Only image files (jpeg, png, gif, webp) are allowed");
        RuleFor(x => x.File.FileSize).LessThanOrEqualTo(MaxFileSize).WithMessage("File size must not exceed 5MB");
    }
}
