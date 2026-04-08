namespace Kickify.Application.Features.PlayerProfiles.Queries.GetMyEloBreakdown;

public record EloBreakdownItemResponse(
    Guid MatchId,
    int EloBefore,
    int EloAfter,
    int EloChange,
    decimal? WinLossComponent,
    decimal? FeedbackComponent,
    decimal? PerformanceComponent,
    decimal? TrustComponent,
    decimal? RoleComponent,
    decimal? SentimentComponent,
    string? CalculationDetails,
    DateTime CreatedAt
);
