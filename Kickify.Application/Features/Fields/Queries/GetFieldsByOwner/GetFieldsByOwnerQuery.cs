using Kickify.Domain.Common;
using MediatR;

namespace Kickify.Application.Features.Fields.Queries.GetFieldsByOwner
{
    public record GetFieldsByOwnerQuery(
        Guid OwnerId,
        int Page = 1,
        int PageSize = 10
    ) : IRequest<Result<GetFieldsByOwnerResponse>>;
}
