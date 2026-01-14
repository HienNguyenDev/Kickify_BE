using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FluentValidation;

namespace Kickify.Application.Features.Posts.Commands.CreatePost
{
    public class CreatePostCommandValidator : AbstractValidator<CreatePostCommand>
    {
        private const int MaxFiles = 10;
        private const long MaxFileSize = 50 * 1024 * 1024; 
        private static readonly string[] AllowedImageTypes = ["image/jpeg", "image/png", "image/gif", "image/webp"];
        private static readonly string[] AllowedVideoTypes = ["video/mp4", "video/quicktime", "video/webm"];

        public CreatePostCommandValidator()
        {
            RuleFor(x => x)
                .Must(x => !string.IsNullOrWhiteSpace(x.Content) || x.Files.Count > 0)
                .WithMessage("Post must have content or at least one media file");

            RuleFor(x => x.Content)
                .MaximumLength(5000)
                .WithMessage("Content must not exceed 5000 characters");

            RuleFor(x => x.Files)
                .Must(files => files.Count <= MaxFiles)
                .WithMessage($"Maximum {MaxFiles} files allowed per post");

            RuleForEach(x => x.Files).ChildRules(file =>
            {
                file.RuleFor(f => f.FileSize)
                    .LessThanOrEqualTo(MaxFileSize)
                    .WithMessage($"File size must not exceed {MaxFileSize / (1024 * 1024)}MB");

                file.RuleFor(f => f.ContentType)
                    .Must(BeValidMediaType)
                    .WithMessage("Invalid file type. Allowed types: JPEG, PNG, GIF, WebP, MP4, QuickTime, WebM");

                file.RuleFor(f => f.FileName)
                    .NotEmpty()
                    .WithMessage("File name is required");
            });
        }

        private static bool BeValidMediaType(string contentType)
        {
            return AllowedImageTypes.Contains(contentType) || AllowedVideoTypes.Contains(contentType);
        }
    }
}
