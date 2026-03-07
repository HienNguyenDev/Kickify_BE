using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;
using Kickify.Domain.Errors;

namespace Kickify.Application.Features.PlayerProfiles.Queries.GetMyRank;

public class GetMyRankQueryHandler : IQueryHandler<GetMyRankQuery, GetMyRankResponse>
{
    private readonly IPlayerProfileRepository _playerProfileRepository;
    private readonly IUserContext _userContext;

    public GetMyRankQueryHandler(
        IPlayerProfileRepository playerProfileRepository,
        IUserContext userContext)
    {
        _playerProfileRepository = playerProfileRepository;
        _userContext = userContext;
    }

    public async Task<Result<GetMyRankResponse>> Handle(GetMyRankQuery request, CancellationToken cancellationToken)
    {
        var userId = _userContext.UserId;

        // Get current user's player profile
        var myProfile = await _playerProfileRepository.GetByUserIdAsync(userId, cancellationToken);
        if (myProfile == null)
        {
            return Result.Failure<GetMyRankResponse>(PlayerProfileErrors.NotFoundByUserId(userId));
        }

        // Get rank
        var rank = await _playerProfileRepository.GetPlayerRankByEloAsync(userId, cancellationToken);

        return Result.Success(new GetMyRankResponse(Rank: rank));
    }
}
