using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.Fields.Commands.DeleteField
{
    public record DeleteFieldCommand(
        Guid FieldId
    ) : ICommand<DeleteFieldResponse>;
}
