namespace Kickify.Application.Features.PlayerProfiles.Queries.GetMyEloBreakdown;

public record EloBreakdownItemResponse(
    Guid MatchId,
    int EloBefore,
    int EloAfter,
    int EloChange,
    decimal? K1MatchResultComponent,
    decimal? K2FeedbackSentimentComponent,
    decimal? K3WinRateComponent,
    decimal? K4ContributionComponent,
    decimal? K5TrustComponent,
    string? CalculationDetails,
    DateTime CreatedAt
);
