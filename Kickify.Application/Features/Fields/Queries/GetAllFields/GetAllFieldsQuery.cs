using Kickify.Domain.Common;
using MediatR;

namespace Kickify.Application.Features.Fields.Queries.GetAllFields
{
    public record GetAllFieldsQuery(
        string? FieldType = null,
        bool? IsActive = null,
        int Page = 1,
        int PageSize = 10
    ) : IRequest<Result<GetAllFieldsResponse>>;
}
