using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Features.PlayerProfiles.Queries.GetPlayerRadarSnapshot;
using Kickify.Domain.Common;
using MediatR;

namespace Kickify.Application.Features.PlayerProfiles.Queries.GetMyRadarSnapshot;

public class GetMyRadarSnapshotQueryHandler : IQueryHandler<GetMyRadarSnapshotQuery, GetMyRadarSnapshotResponse>
{
    private readonly ISender _sender;
    private readonly IUserContext _userContext;

    public GetMyRadarSnapshotQueryHandler(ISender sender, IUserContext userContext)
    {
        _sender = sender;
        _userContext = userContext;
    }

    public Task<Result<GetMyRadarSnapshotResponse>> Handle(
        GetMyRadarSnapshotQuery request,
        CancellationToken cancellationToken) =>
        _sender.Send(new GetPlayerRadarSnapshotQuery(_userContext.UserId), cancellationToken);
}
