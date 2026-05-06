using FluentValidation;

namespace Kickify.Application.Features.EloConfigurations.Commands.UpdateActiveEloConfiguration;

public class UpdateActiveEloConfigurationCommandValidator : AbstractValidator<UpdateActiveEloConfigurationCommand>
{
    public UpdateActiveEloConfigurationCommandValidator()
    {
        RuleFor(x => x.K1MatchResult).GreaterThanOrEqualTo(0);
        RuleFor(x => x.K2FeedbackSentiment).GreaterThanOrEqualTo(0);
        RuleFor(x => x.K3WinStreak).GreaterThanOrEqualTo(0);
        RuleFor(x => x.K4Contribution).GreaterThanOrEqualTo(0);
        RuleFor(x => x.K5Trust).GreaterThanOrEqualTo(0);
    }
}
