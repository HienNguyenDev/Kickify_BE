namespace Kickify.Api.Requests;

public sealed record UpdateActiveEloConfigurationRequest(
    decimal K1MatchResult,
    decimal K2FeedbackSentiment,
    decimal K3WinStreak,
    decimal K4Contribution,
    decimal K5Trust
);
