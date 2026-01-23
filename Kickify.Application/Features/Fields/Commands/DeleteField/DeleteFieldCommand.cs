using Kickify.Domain.Common;
using MediatR;

namespace Kickify.Application.Features.Fields.Commands.DeleteField
{
    public record DeleteFieldCommand(
        Guid FieldId,
        Guid UserId
    ) : IRequest<Result<DeleteFieldResponse>>;
}
