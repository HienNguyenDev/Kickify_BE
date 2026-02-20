using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Kickify.Application.Abstractions.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Kickify.Infrastructure.Services;

public class SentimentAnalysisService : ISentimentAnalysisService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<SentimentAnalysisService> _logger;

    public SentimentAnalysisService(
        HttpClient httpClient,
        ILogger<SentimentAnalysisService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task SendFeedbacksForAnalysisAsync(
        SentimentBatchRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Map to the AI API's expected JSON format (snake_case)
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
                cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation(
                    "Successfully sent feedbacks for player {PlayerId} in match {MatchId} to AI",
                    request.TargetPlayerId, request.MatchId);
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogError(
                    "AI sentiment analysis API returned {StatusCode} for player {PlayerId} in match {MatchId}. Response: {Error}",
                    response.StatusCode, request.TargetPlayerId, request.MatchId, errorContent);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Failed to send feedbacks for player {PlayerId} in match {MatchId} to AI sentiment analysis",
                request.TargetPlayerId, request.MatchId);
        }
    }
}
