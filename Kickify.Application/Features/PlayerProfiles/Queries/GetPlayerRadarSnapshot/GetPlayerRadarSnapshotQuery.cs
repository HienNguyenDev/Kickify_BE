using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Features.PlayerProfiles.Queries.GetMyRadarSnapshot;

namespace Kickify.Application.Features.PlayerProfiles.Queries.GetPlayerRadarSnapshot;

public record GetPlayerRadarSnapshotQuery(Guid UserId) : IQuery<GetMyRadarSnapshotResponse>;
