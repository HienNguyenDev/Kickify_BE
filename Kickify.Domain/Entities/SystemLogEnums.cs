namespace Kickify.Domain.Entities;

public enum SystemLogAction
{
    Create = 1,
    Update = 2,
    Delete = 3
}

public enum SystemLogResponseStatus
{
    Success = 1,       // 2xx
    Error = 2,         // 4xx
    ServerFailure = 3  // 5xx
}
