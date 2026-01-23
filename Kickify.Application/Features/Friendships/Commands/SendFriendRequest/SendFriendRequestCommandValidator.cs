using FluentValidation;

namespace Kickify.Application.Features.Friendships.Commands.SendFriendRequest;

public class SendFriendRequestCommandValidator : AbstractValidator<SendFriendRequestCommand>
{
    public SendFriendRequestCommandValidator()
    {
        RuleFor(x => x.AddresseeId).NotEmpty().WithMessage("AddresseeId is required");
    }
}
