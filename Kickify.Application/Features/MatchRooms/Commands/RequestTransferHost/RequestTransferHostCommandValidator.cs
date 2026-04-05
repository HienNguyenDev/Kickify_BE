using FluentValidation;

namespace Kickify.Application.Features.MatchRooms.Commands.RequestTransferHost;

public sealed class RequestTransferHostCommandValidator : AbstractValidator<RequestTransferHostCommand>
{
    public RequestTransferHostCommandValidator()
    {
        RuleFor(x => x.RoomId).NotEmpty();
        RuleFor(x => x.TargetUserId).NotEmpty();
    }
}