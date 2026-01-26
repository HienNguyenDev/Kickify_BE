using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.Fields.Queries.GetFieldById
{
    public record GetFieldByIdQuery(Guid FieldId) : IQuery<GetFieldByIdResponse>;
}
