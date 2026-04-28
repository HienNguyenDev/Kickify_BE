using System.Net.Http.Json;
using System.Text.Json;
using Kickify.Application.Abstractions.Services;
using Microsoft.Extensions.Logging;

namespace Kickify.Infrastructure.Services;

public class AiSuggestionService : IAiSuggestionService
{
    private const int MaxRetries = 3;
    private readonly HttpClient _httpClient;
    private readonly ILogger<AiSuggestionService> _logger;

    private static readonly JsonSerializerOptions SnakeCaseOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true
    };

    public AiSuggestionService(HttpClient httpClient, ILogger<AiSuggestionService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<RoomQueryParseResult?> ParseRoomQueryAsync(
        RoomQueryParseRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = new
            {
                query = request.Query,
                current_date = request.CurrentDate,
                current_time = request.CurrentTime,
                user_latitude = request.UserLatitude,
                user_longitude = request.UserLongitude,
            };

            var response = await PostWithRetryAsync("/api/suggestions/rooms", payload, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("AI suggestion/rooms returned {Status}: {Error}", response.StatusCode, error);
                return null;
            }

            var dto = await response.Content.ReadFromJsonAsync<RoomSuggestionDto>(SnakeCaseOptions, cancellationToken);
            if (dto?.ParsedFilter is null) return null;

            var f = dto.ParsedFilter;

            DateTime? date = null;
            if (!string.IsNullOrWhiteSpace(f.Date) && DateTime.TryParse(f.Date, out var parsedDate))
                date = parsedDate.Date;

            TimeSpan? timeFrom = null;
            if (!string.IsNullOrWhiteSpace(f.TimeFrom) && TimeSpan.TryParse(f.TimeFrom, out var tf))
                timeFrom = tf;

            TimeSpan? timeTo = null;
            if (!string.IsNullOrWhiteSpace(f.TimeTo) && TimeSpan.TryParse(f.TimeTo, out var tt))
                timeTo = tt;

            return new RoomQueryParseResult(
                IsRelevant: f.IsRelevant,
                MatchFormat: string.IsNullOrWhiteSpace(f.MatchFormat) ? null : f.MatchFormat,
                Date: date,
                TimeFrom: timeFrom,
                TimeTo: timeTo,
                LocationName: string.IsNullOrWhiteSpace(f.LocationName) ? null : f.LocationName,
                AvailableOnly: f.AvailableOnly);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to call AI suggestion/rooms");
            return null;
        }
    }

    public async Task<PlayerQueryParseResult?> ParsePlayerQueryAsync(
        PlayerQueryParseRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = new
            {
                query = request.Query,
                current_date = request.CurrentDate,
                exclude_player_ids = request.ExcludePlayerIds,
            };

            var response = await PostWithRetryAsync("/api/suggestions/players", payload, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("AI suggestion/players returned {Status}: {Error}", response.StatusCode, error);
                return null;
            }

            var dto = await response.Content.ReadFromJsonAsync<PlayerSuggestionDto>(SnakeCaseOptions, cancellationToken);
            if (dto?.ParsedFilter is null) return null;

            var f = dto.ParsedFilter;

            return new PlayerQueryParseResult(
                IsRelevant: f.IsRelevant,
                Positions: f.Positions is { Count: > 0 } ? f.Positions : null,
                MinElo: f.MinElo,
                MaxElo: f.MaxElo,
                PreferredFoot: string.IsNullOrWhiteSpace(f.PreferredFoot) ? null : f.PreferredFoot,
                HighFormOnly: f.HighFormOnly,
                MinTrustScore: f.MinTrustScore,
                SimilarPlayerIds: dto.SimilarPlayerIds ?? []);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to call AI suggestion/players");
            return null;
        }
    }

    private async Task<HttpResponseMessage> PostWithRetryAsync(
        string endpoint,
        object payload,
        CancellationToken cancellationToken)
    {
        var requestId = Guid.NewGuid().ToString("N");
        var delayMs = 250;
        HttpResponseMessage? lastResponse = null;

        for (var attempt = 1; attempt <= MaxRetries; attempt++)
        {
            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, endpoint)
            {
                Content = JsonContent.Create(payload, options: SnakeCaseOptions)
            };
            httpRequest.Headers.Add("x-request-id", requestId);

            try
            {
                lastResponse = await _httpClient.SendAsync(httpRequest, cancellationToken);
                if ((int)lastResponse.StatusCode < 500 || attempt == MaxRetries)
                    return lastResponse;
            }
            catch (HttpRequestException) when (attempt < MaxRetries)
            {
                // Retry transient network failures
            }

            _logger.LogWarning(
                "AI suggestion request retry {Attempt}/{Max} for {Endpoint}, request_id={RequestId}",
                attempt + 1, MaxRetries, endpoint, requestId);

            await Task.Delay(delayMs, cancellationToken);
            delayMs *= 2;
        }

        return lastResponse ?? new HttpResponseMessage(System.Net.HttpStatusCode.ServiceUnavailable);
    }

    // ─── Private DTO shapes (match FastAPI snake_case response) ──────────────

    private sealed class RoomSuggestionDto
    {
        public RoomFilterDto? ParsedFilter { get; set; }
        public string? RawQuery { get; set; }
        public string? ParseSource { get; set; }
    }

    private sealed class RoomFilterDto
    {
        public bool IsRelevant { get; set; }
        public string? MatchFormat { get; set; }
        public string? Date { get; set; }
        public string? TimeFrom { get; set; }
        public string? TimeTo { get; set; }
        public string? LocationName { get; set; }
        public bool AvailableOnly { get; set; } = true;
    }

    private sealed class PlayerSuggestionDto
    {
        public PlayerFilterDto? ParsedFilter { get; set; }
        public string? RawQuery { get; set; }
        public List<string>? SimilarPlayerIds { get; set; }
        public string? ParseSource { get; set; }
    }

    private sealed class PlayerFilterDto
    {
        public bool IsRelevant { get; set; }
        public List<string>? Positions { get; set; }
        public int? MinElo { get; set; }
        public int? MaxElo { get; set; }
        public string? PreferredFoot { get; set; }
        public bool HighFormOnly { get; set; }
        public decimal? MinTrustScore { get; set; }
    }
}
