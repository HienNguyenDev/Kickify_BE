namespace Kickify.Application.Features.EloConfigurations.Commands.UpdateActiveEloConfiguration;

public sealed record UpdateActiveEloConfigurationResponse(
    Guid ConfigId,
    decimal K1MatchResult,
    decimal K2FeedbackSentiment,
    decimal K3WinRate,
    decimal K4Contribution,
    decimal K5Trust,
    bool IsActive,
    DateTime UpdatedAt
);
