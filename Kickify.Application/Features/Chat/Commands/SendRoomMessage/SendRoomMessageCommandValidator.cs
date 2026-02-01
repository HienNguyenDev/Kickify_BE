using FluentValidation;

namespace Kickify.Application.Features.Chat.Commands.SendRoomMessage;

public class SendRoomMessageCommandValidator : AbstractValidator<SendRoomMessageCommand>
{
    public SendRoomMessageCommandValidator()
    {
        RuleFor(x => x.RoomId).NotEmpty().WithMessage("RoomId is required");
        RuleFor(x => x.Channel).IsInEnum().WithMessage("Invalid channel");
        RuleFor(x => x.MessageText).NotEmpty().MaximumLength(2000).WithMessage("Message must be between 1 and 2000 characters");
    }
}
