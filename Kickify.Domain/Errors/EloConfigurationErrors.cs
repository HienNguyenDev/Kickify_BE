using Kickify.Domain.Common;

namespace Kickify.Domain.Errors;

public static class EloConfigurationErrors
{
    public static readonly Error ActiveNotFound = Error.NotFound(
        "EloConfigurations.ActiveNotFound",
        "Active ELO configuration was not found.");
}
