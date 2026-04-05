using FluentValidation;

namespace Kickify.Application.Features.MatchRooms.Commands.RespondTransferHost;

public sealed class RespondTransferHostCommandValidator : AbstractValidator<RespondTransferHostCommand>
{
    public RespondTransferHostCommandValidator()
    {
        RuleFor(x => x.RoomId).NotEmpty();
        RuleFor(x => x.IsAccepted).NotNull();
    }
}