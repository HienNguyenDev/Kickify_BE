using Kickify.Application.Abstractions.Authentication;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Kickify.Infrastructure.Authentication
{
    public class UserContext : IUserContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserContext(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public Guid UserId
        {
            get
            {
                var httpContext = _httpContextAccessor.HttpContext;

                // Try getting from HttpContext.Items first (set by SignalR Hub)
                if (httpContext?.Items.TryGetValue("SignalR_UserId", out var signalRUserId) == true
                    && signalRUserId is Guid hubUserId)
                {
                    return hubUserId;
                }

                // Fallback to standard ClaimsPrincipal (HTTP request)
                return httpContext?.User?.GetUserId()
                    ?? throw new ApplicationException("User context is unavailable");
            }
        }
    }
}
