using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.EloConfigurations.Commands.UpdateActiveEloConfiguration;

public sealed record UpdateActiveEloConfigurationCommand(
    decimal K1MatchResult,
    decimal K2FeedbackSentiment,
    decimal K3WinStreak,
    decimal K4Contribution,
    decimal K5Trust
) : ICommand<UpdateActiveEloConfigurationResponse>;
