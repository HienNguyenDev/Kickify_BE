using System.Security.Claims;
using Kickify.Application.Abstractions.Authentication;
using Microsoft.AspNetCore.Http;

namespace Kickify.Infrastructure.Authentication;

public sealed class CurrentUserReader : ICurrentUserReader
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserReader(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public Guid? TryGetUserId()
    {
        var principal = _httpContextAccessor.HttpContext?.User;
        if (principal?.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        var raw = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(raw, out var id) ? id : null;
    }
}
