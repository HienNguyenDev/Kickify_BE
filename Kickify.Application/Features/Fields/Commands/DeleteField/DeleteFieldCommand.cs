using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.Fields.Commands.DeleteField
{
    public record DeleteFieldCommand(
        Guid FieldId,
        Guid UserId
    ) : ICommand<DeleteFieldResponse>;
}
