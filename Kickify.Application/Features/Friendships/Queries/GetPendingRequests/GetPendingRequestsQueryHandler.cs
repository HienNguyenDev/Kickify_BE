using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Domain.Common;

namespace Kickify.Application.Features.Friendships.Queries.GetPendingRequests;

public class GetPendingRequestsQueryHandler : IQueryHandler<GetPendingRequestsQuery, GetPendingRequestsQueryResponse>
{
    private readonly IFriendshipRepository _friendshipRepository;
    private readonly IUserContext _userContext;

    public GetPendingRequestsQueryHandler(IFriendshipRepository friendshipRepository, IUserContext userContext)
    {
        _friendshipRepository = friendshipRepository;
        _userContext = userContext;
    }

    public async Task<Result<GetPendingRequestsQueryResponse>> Handle(GetPendingRequestsQuery request, CancellationToken cancellationToken)
    {
        var (requests, total) = await _friendshipRepository.GetPendingRequestsAsync(_userContext.UserId, request.Page, request.PageSize, cancellationToken);

        var requestDtos = requests.Select(r => new FriendRequestDto
        {
            FriendshipId = r.FriendshipId,
            RequesterId = r.RequesterId,
            RequesterFullName = r.Requester?.FullName ?? string.Empty,
            RequesterAvatarUrl = r.Requester?.AvatarUrl,
            SentAt = r.CreatedAt
        }).ToList();

        var response = new GetPendingRequestsQueryResponse
        {
            Requests = requestDtos,
            TotalCount = total,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling(total / (double)request.PageSize)
        };

        return Result.Success(response);
    }
}
