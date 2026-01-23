using FluentValidation;

namespace Kickify.Application.Features.Friendships.Commands.RespondFriendRequest;

public class RespondFriendRequestCommandValidator : AbstractValidator<RespondFriendRequestCommand>
{
    public RespondFriendRequestCommandValidator()
    {
        RuleFor(x => x.FriendshipId).NotEmpty().WithMessage("FriendshipId is required");
    }
}
