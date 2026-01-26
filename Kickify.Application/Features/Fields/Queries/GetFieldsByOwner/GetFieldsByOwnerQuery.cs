using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.Fields.Queries.GetFieldsByOwner
{
    public record GetFieldsByOwnerQuery(
        Guid OwnerId,
        int Page = 1,
        int PageSize = 10
    ) : IQuery<GetFieldsByOwnerResponse>;
}
