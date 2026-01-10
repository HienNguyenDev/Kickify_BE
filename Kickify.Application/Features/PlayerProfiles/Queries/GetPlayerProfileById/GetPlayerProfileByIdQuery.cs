using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.PlayerProfiles.Queries.GetPlayerProfileById
{
    public class GetPlayerProfileByIdQuery : IQuery<GetPlayerProfileByIdQueryResponse>
    {
        public Guid ProfileId { get; set; }
    }
}
