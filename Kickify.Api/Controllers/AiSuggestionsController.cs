using Kickify.Api.Extensions;
using Kickify.Application.Features.AiSuggestions.Queries.SuggestMatchRooms;
using Kickify.Application.Features.AiSuggestions.Queries.SuggestPlayers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Kickify.Api.Controllers;

[ApiController]
[Route("api/ai")]
[Authorize]
public class AiSuggestionsController : ControllerBase
{
    private readonly ISender _sender;

    public AiSuggestionsController(ISender sender)
    {
        _sender = sender;
    }

    /// <summary>
    /// Parse a natural-language Vietnamese query and return matching open match rooms.
    /// Example queries: "Tìm sân 5vs5 tối nay gần Cầu Giấy", "7v7 cuối tuần này".
    /// The caller may optionally pass GPS coordinates (from device) to enable geo-filtering.
    /// When the query is irrelevant a 400 is returned with a Vietnamese explanation.
    /// Results share the same shape as GET /api/match-rooms — use POST /api/match-rooms/{id}/join to join.
    /// </summary>
    [HttpPost("suggest-rooms")]
    public async Task<IResult> SuggestRooms(
        [FromBody] SuggestRoomsRequest request,
        CancellationToken cancellationToken)
    {
        var query = new SuggestMatchRoomsQuery(
            Query: request.Query,
            Latitude: request.Latitude,
            Longitude: request.Longitude,
            Page: request.Page,
            PageSize: request.PageSize);

        var result = await _sender.Send(query, cancellationToken);
        return result.MatchOk();
    }

    /// <summary>
    /// Parse a natural-language Vietnamese query and return matching player profiles.
    /// Example queries: "Cần tiền vệ ELO 1400+ thuận chân trái đang form tốt", "Tìm thủ môn uy tín cao".
    /// When the query is irrelevant a 400 is returned with a Vietnamese explanation.
    /// Results share the same shape as GET /api/playerprofiles.
    /// To invite a player use POST /api/match-rooms/{roomId}/invitations with their userId.
    /// </summary>
    [HttpPost("suggest-players")]
    public async Task<IResult> SuggestPlayers(
        [FromBody] SuggestPlayersRequest request,
        CancellationToken cancellationToken)
    {
        var query = new SuggestPlayersQuery(
            Query: request.Query,
            RoomId: request.RoomId,
            Page: request.Page,
            PageSize: request.PageSize);

        var result = await _sender.Send(query, cancellationToken);
        return result.MatchOk();
    }
}

public record SuggestRoomsRequest(
    string Query,
    double? Latitude,
    double? Longitude,
    int Page = 1,
    int PageSize = 10);

public record SuggestPlayersRequest(
    string Query,
    Guid? RoomId,
    int Page = 1,
    int PageSize = 10);
