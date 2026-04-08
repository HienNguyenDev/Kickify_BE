using System.Net.Http.Json;
using System.Text.Json;
using Kickify.Application.Abstractions.Services;
using Microsoft.Extensions.Logging;

namespace Kickify.Infrastructure.Services;

public class SentimentAnalysisService : ISentimentAnalysisService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<SentimentAnalysisService> _logger;
    private static readonly JsonSerializerOptions SnakeCaseOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true
    };

    public SentimentAnalysisService(
        HttpClient httpClient,
        ILogger<SentimentAnalysisService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<SentimentBatchResponse?> SendFeedbacksForAnalysisAsync(
        SentimentBatchRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = new
            {
                match_id = request.MatchId,
                target_player_id = request.TargetPlayerId,
                feedbacks = request.Feedbacks.Select(f => new
                {
                    feedback_id = f.FeedbackId,
                    reviewer_id = f.ReviewerId,
                    comment = f.Comment,
                    star_rating = f.StarRating
                }).ToList()
            };

            _logger.LogInformation(
                "Sending {FeedbackCount} feedbacks for player {PlayerId} in match {MatchId} to AI sentiment analysis",
                request.Feedbacks.Count, request.TargetPlayerId, request.MatchId);

            var response = await _httpClient.PostAsJsonAsync(
                "/api/sentiment/analyze-batch",
                payload,
                SnakeCaseOptions,
                cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError(
                    "AI sentiment analysis API returned {StatusCode} for player {PlayerId} in match {MatchId}. Response: {Error}",
                    response.StatusCode, request.TargetPlayerId, request.MatchId, errorContent);
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<SentimentBatchResponse>(SnakeCaseOptions, cancellationToken);
            _logger.LogInformation(
                "Successfully sent feedbacks for player {PlayerId} in match {MatchId} to AI",
                request.TargetPlayerId, request.MatchId);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to send feedbacks for player {PlayerId} in match {MatchId} to AI sentiment analysis",
                request.TargetPlayerId, request.MatchId);
            return null;
        }
    }

    public async Task<EloCalculationResponse?> CalculateEloAsync(EloCalculationRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/elo/calculate", request, SnakeCaseOptions, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("AI elo calculate failed: {StatusCode} - {Error}", response.StatusCode, errorContent);
                return null;
            }

            return await response.Content.ReadFromJsonAsync<EloCalculationResponse>(SnakeCaseOptions, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to call AI elo calculate API");
            return null;
        }
    }

    public async Task<RadarAnalysisResponse?> AnalyzeRadarAsync(RadarAnalysisRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/radar/analyze", request, SnakeCaseOptions, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError("AI radar analyze failed: {StatusCode} - {Error}", response.StatusCode, errorContent);
                return null;
            }

            return await response.Content.ReadFromJsonAsync<RadarAnalysisResponse>(SnakeCaseOptions, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to call AI radar analyze API");
            return null;
        }
    }
}
