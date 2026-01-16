using FluentValidation;

namespace Kickify.Application.Features.Chat.Commands.SendPrivateMessage;

public class SendPrivateMessageCommandValidator : AbstractValidator<SendPrivateMessageCommand>
{
    public SendPrivateMessageCommandValidator()
    {
        RuleFor(x => x.ReceiverId)
            .NotEmpty()
            .WithMessage("ReceiverId is required");

        RuleFor(x => x.MessageText)
            .NotEmpty()
            .WithMessage("Message text is required")
            .MaximumLength(2000)
            .WithMessage("Message text must not exceed 2000 characters");
    }
}
