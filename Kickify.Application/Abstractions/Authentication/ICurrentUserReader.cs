namespace Kickify.Application.Abstractions.Authentication;

/// <summary>
/// Reads the current HTTP user when present, without throwing for anonymous requests.
/// </summary>
public interface ICurrentUserReader
{
    Guid? TryGetUserId();
}
