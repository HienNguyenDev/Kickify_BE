using Kickify.Application.Abstractions.Messaging;
using Kickify.Domain.Enums;

namespace Kickify.Application.Features.Users.Queries.GetUserById
{
    public class GetUserByIdQuery : IQuery<GetUserByIdQueryResponse>
    {
        public Guid UserId { get; set; }
    }
}
