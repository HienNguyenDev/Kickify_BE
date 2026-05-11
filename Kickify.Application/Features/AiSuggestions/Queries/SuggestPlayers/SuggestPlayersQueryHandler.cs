using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Abstractions.Services;
using Kickify.Application.Features.PlayerProfiles.Queries.GetAllPlayerProfiles;
using Kickify.Domain.Common;
using Kickify.Domain.Errors;
using MediatR;

namespace Kickify.Application.Features.AiSuggestions.Queries.SuggestPlayers;

public class SuggestPlayersQueryHandler
    : IQueryHandler<SuggestPlayersQuery, GetAllPlayerProfilesQueryResponse>
{
    private readonly IAiSuggestionService _aiSuggestionService;
    private readonly ISender _sender;
    private readonly IUserRepository _userRepository;
    private readonly IUserContext _userContext;

    public SuggestPlayersQueryHandler(
        IAiSuggestionService aiSuggestionService,
        ISender sender,
        IUserRepository userRepository,
        IUserContext userContext)
    {
        _aiSuggestionService = aiSuggestionService;
        _sender = sender;
        _userRepository = userRepository;
        _userContext = userContext;
    }

    public async Task<Result<GetAllPlayerProfilesQueryResponse>> Handle(
        SuggestPlayersQuery request,
        CancellationToken cancellationToken)
    {
        // ── Premium guard ──────────────────────────────────────────────────
        var user = await _userRepository.GetByIdAsync(_userContext.UserId);
        if (user is null || !user.IsPremium)
            return Result.Failure<GetAllPlayerProfilesQueryResponse>(PremiumErrors.PremiumRequired);
        if (user.PremiumExpireAt.HasValue && user.PremiumExpireAt < DateTime.UtcNow)
            return Result.Failure<GetAllPlayerProfilesQueryResponse>(PremiumErrors.PremiumExpired);

        var now = DateTime.UtcNow.AddHours(7); // GMT+7

        var parseResult = await _aiSuggestionService.ParsePlayerQueryAsync(
            new PlayerQueryParseRequest(
                Query: request.Query,
                CurrentDate: now.ToString("yyyy-MM-dd")),
            cancellationToken);

        if (parseResult is null)
        {
            return Result.Failure<GetAllPlayerProfilesQueryResponse>(
                Error.Failure("AiSuggestion.ServiceUnavailable",
                    "Dịch vụ AI tạm thời không khả dụng. Vui lòng thử lại sau."));
        }

        if (!parseResult.IsRelevant)
        {
            return Result.Failure<GetAllPlayerProfilesQueryResponse>(
                Error.Failure("AiSuggestion.NotRelevant",
                    "Yêu cầu không liên quan đến tìm cầu thủ bóng đá. Vui lòng nhập lại."));
        }

        var searchQuery = new GetAllPlayerProfilesQuery
        {
            MinElo = parseResult.MinElo,
            MaxElo = parseResult.MaxElo,
            MinTrustScore = parseResult.MinTrustScore,
            Positions = parseResult.Positions,
            PreferredFoot = parseResult.PreferredFoot,
            HighFormOnly = parseResult.HighFormOnly,
            PrioritisePlayerIds = parseResult.SimilarPlayerIds,
            Page = request.Page,
            PageSize = request.PageSize,
        };

        return await _sender.Send(searchQuery, cancellationToken);
    }
}
