using FluentValidation;

namespace Kickify.Application.Features.Friendships.Commands.RespondFriendRequest;

public class RespondFriendRequestCommandValidator : AbstractValidator<RespondFriendRequestCommand>
{
    public RespondFriendRequestCommandValidator()
    {
        RuleFor(x => x.RequesterId).NotEmpty().WithMessage("RequesterId is required");
    }
}
