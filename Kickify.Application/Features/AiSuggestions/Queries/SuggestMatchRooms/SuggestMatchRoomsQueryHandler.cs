using Kickify.Application.Abstractions.Authentication;
using Kickify.Application.Abstractions.Messaging;
using Kickify.Application.Abstractions.Repositories;
using Kickify.Application.Abstractions.Services;
using Kickify.Application.Features.MatchRooms.Queries.GetMatchRooms;
using Kickify.Domain.Common;
using Kickify.Domain.Errors;
using MediatR;

namespace Kickify.Application.Features.AiSuggestions.Queries.SuggestMatchRooms;

public class SuggestMatchRoomsQueryHandler
    : IQueryHandler<SuggestMatchRoomsQuery, GetMatchRoomsResponse>
{
    private readonly IAiSuggestionService _aiSuggestionService;
    private readonly ISender _sender;
    private readonly IUserRepository _userRepository;
    private readonly IUserContext _userContext;

    public SuggestMatchRoomsQueryHandler(
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

    public async Task<Result<GetMatchRoomsResponse>> Handle(
        SuggestMatchRoomsQuery request,
        CancellationToken cancellationToken)
    {
        // ── Premium guard ──────────────────────────────────────────────────
        var user = await _userRepository.GetByIdAsync(_userContext.UserId);
        if (user is null || !user.IsPremium)
            return Result.Failure<GetMatchRoomsResponse>(PremiumErrors.PremiumRequired);
        if (user.PremiumExpireAt.HasValue && user.PremiumExpireAt < DateTime.UtcNow)
            return Result.Failure<GetMatchRoomsResponse>(PremiumErrors.PremiumExpired);

        var now = DateTime.UtcNow.AddHours(7); // GMT+7

        var parseResult = await _aiSuggestionService.ParseRoomQueryAsync(
            new RoomQueryParseRequest(
                Query: request.Query,
                CurrentDate: now.ToString("yyyy-MM-dd"),
                CurrentTime: now.ToString("HH:mm"),
                UserLatitude: request.Latitude,
                UserLongitude: request.Longitude),
            cancellationToken);

        if (parseResult is null)
        {
            return Result.Failure<GetMatchRoomsResponse>(
                Error.Failure("AiSuggestion.ServiceUnavailable",
                    "Dịch vụ AI tạm thời không khả dụng. Vui lòng thử lại sau."));
        }

        if (!parseResult.IsRelevant)
        {
            return Result.Failure<GetMatchRoomsResponse>(
                Error.Failure("AiSuggestion.NotRelevant",
                    "Yêu cầu không liên quan đến tìm phòng đấu bóng đá. Vui lòng nhập lại."));
        }

        // Use GPS coords from the request directly if provided; fall back to those
        // that may have been derived from location_name geocoding on the client side.
        var latitude = request.Latitude.HasValue ? (decimal?)request.Latitude.Value : null;
        var longitude = request.Longitude.HasValue ? (decimal?)request.Longitude.Value : null;

        var searchQuery = new GetMatchRoomsQuery(
            Dates: parseResult.Dates,
            MatchFormat: parseResult.MatchFormat,
            AvailableOnly: parseResult.AvailableOnly,
            Latitude: latitude,
            Longitude: longitude,
            RadiusKm: latitude.HasValue ? 10.0 : null,
            Page: request.Page,
            PageSize: request.PageSize);

        return await _sender.Send(searchQuery, cancellationToken);
    }
}
