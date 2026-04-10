using Kickify.Application.Abstractions.Messaging;

namespace Kickify.Application.Features.EloConfigurations.Queries.GetActiveEloConfiguration;

public sealed record GetActiveEloConfigurationQuery() : IQuery<GetActiveEloConfigurationResponse>;
