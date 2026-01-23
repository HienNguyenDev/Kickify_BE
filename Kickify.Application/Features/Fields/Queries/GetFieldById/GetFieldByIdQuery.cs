using Kickify.Domain.Common;
using MediatR;

namespace Kickify.Application.Features.Fields.Queries.GetFieldById
{
    public record GetFieldByIdQuery(Guid FieldId) : IRequest<Result<GetFieldByIdResponse>>;
}
