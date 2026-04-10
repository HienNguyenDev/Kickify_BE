namespace Kickify.Application.Features.EloConfigurations.Queries.GetActiveEloConfiguration;

public sealed record GetActiveEloConfigurationResponse(
    Guid ConfigId,
    decimal K1MatchResult,
    decimal K2FeedbackSentiment,
    decimal K3WinRate,
    decimal K4Contribution,
    decimal K5Trust,
    bool IsActive,
    DateTime UpdatedAt
);
