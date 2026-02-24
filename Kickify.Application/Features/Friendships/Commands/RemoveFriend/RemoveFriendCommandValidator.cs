using FluentValidation;

namespace Kickify.Application.Features.Friendships.Commands.RemoveFriend;

public class RemoveFriendCommandValidator : AbstractValidator<RemoveFriendCommand>
{
    public RemoveFriendCommandValidator()
    {
        RuleFor(x => x.FriendId).NotEmpty().WithMessage("FriendId is required");
    }
}
