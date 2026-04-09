using System.Security.Claims;
using Kickify.Application.Abstractions.Services;
using Kickify.Domain.Entities;
using Microsoft.AspNetCore.Routing;

namespace Kickify.Api.Middleware;

public sealed class SystemAuditLoggingMiddleware(RequestDelegate next)
{
    private static readonly Dictionary<string, string> EntityTypeByRouteSegment = new(StringComparer.OrdinalIgnoreCase)
    {
        ["users"] = "User",
        ["authenticate"] = "Auth",
        ["venues"] = "Venue",
        ["venue-photos"] = "VenuePhoto",
        ["venue-reviews"] = "VenueReview",
        ["holidays"] = "Holiday",
        ["fields"] = "Field",
        ["bookings"] = "Booking",
        ["match-rooms"] = "MatchRoom",
        ["match-presets"] = "MatchPreset",
        ["match-feedbacks"] = "MatchFeedback",
        ["matchfeedbacks"] = "MatchFeedback",
        ["playerprofiles"] = "PlayerProfile",
        ["friendships"] = "Friendship",
        ["posts"] = "Post",
        ["comments"] = "Comment",
        ["chat"] = "Chat",
        ["wallets"] = "Wallet",
        ["withdrawals"] = "Withdrawal",
        ["notifications"] = "Notification",
        ["notification-preferences"] = "NotificationPreference",
        ["announcements"] = "Announcement",
        ["content-reports"] = "ContentReport",
        ["player-reports"] = "PlayerReport",
        ["analytics"] = "Analytics",
        ["achievements"] = "Achievement"
    };

    public async Task Invoke(HttpContext context, ISystemLogQueue logQueue, ILogger<SystemAuditLoggingMiddleware> logger)
    {
        if (IsExcludedPath(context.Request.Path))
        {
            await next(context);
            return;
        }

        var action = ResolveAction(context.Request.Method);
        if (action is null)
        {
            await next(context);
            return;
        }

        await next(context);

        var entityType = ResolveEntityType(context.Request.Path);
        if (entityType is null)
        {
            return;
        }

        var status = ResolveResponseStatus(context.Response.StatusCode);
        var (userId, userName) = ResolveUser(context.User);
        var userAgent = ResolveClientPlatform(context);
        var entityId = ResolveEntityId(context.Request.RouteValues);
        var errorMessage = status == SystemLogResponseStatus.Success
            ? null
            : BuildErrorMessage(status, context.Response.StatusCode);

        var enqueued = logQueue.TryEnqueue(new SystemLogQueueItem(
            userId,
            userName,
            action.Value,
            entityType,
            entityId,
            userAgent,
            status,
            errorMessage,
            DateTime.UtcNow));

        if (!enqueued)
        {
            logger.LogWarning("SystemLog queue is full. Dropped audit log for {Method} {Path}", context.Request.Method, context.Request.Path);
        }
    }

    private static bool IsExcludedPath(PathString path)
    {
        return path.StartsWithSegments("/health", StringComparison.OrdinalIgnoreCase)
               || path.StartsWithSegments("/swagger", StringComparison.OrdinalIgnoreCase)
               || path.StartsWithSegments("/metrics", StringComparison.OrdinalIgnoreCase)
               || path.StartsWithSegments("/hangfire", StringComparison.OrdinalIgnoreCase)
               || path.StartsWithSegments("/api/authenticate/auth/login-with-refresh-token", StringComparison.OrdinalIgnoreCase);
    }

    private static SystemLogAction? ResolveAction(string method)
    {
        return method.ToUpperInvariant() switch
        {
            "POST" => SystemLogAction.Create,
            "PUT" => SystemLogAction.Update,
            "PATCH" => SystemLogAction.Update,
            "DELETE" => SystemLogAction.Delete,
            _ => null
        };
    }

    private static string? ResolveEntityType(PathString path)
    {
        var segments = path.Value?
            .Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (segments is null || segments.Length < 2 || !segments[0].Equals("api", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        // Prefer exact mapped resource from any API segment for nested routes (e.g. /api/posts/{id}/comments).
        foreach (var rawSegment in segments.Skip(1))
        {
            var key = rawSegment.ToLowerInvariant();
            if (EntityTypeByRouteSegment.TryGetValue(key, out var mapped))
            {
                return mapped;
            }
        }

        // Fallback: use the first resource segment after /api.
        var first = segments[1];
        return string.IsNullOrWhiteSpace(first)
            ? null
            : ToPascalCase(first);
    }

    private static string ToPascalCase(string value)
    {
        var tokens = value
            .Split(new[] { '-', '_' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return string.Concat(tokens.Select(t => char.ToUpperInvariant(t[0]) + t[1..]));
    }

    private static SystemLogResponseStatus ResolveResponseStatus(int statusCode)
    {
        if (statusCode is >= 200 and < 300)
        {
            return SystemLogResponseStatus.Success;
        }

        if (statusCode is >= 400 and < 500)
        {
            return SystemLogResponseStatus.Error;
        }

        return SystemLogResponseStatus.ServerFailure;
    }

    private static string BuildErrorMessage(SystemLogResponseStatus status, int statusCode)
    {
        return status switch
        {
            SystemLogResponseStatus.Error => $"Client error ({statusCode})",
            SystemLogResponseStatus.ServerFailure => $"Server failure ({statusCode})",
            _ => string.Empty
        };
    }

    private static (Guid? UserId, string? UserName) ResolveUser(ClaimsPrincipal principal)
    {
        Guid? userId = null;
        var idValue = principal.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? principal.FindFirstValue("sub")
                      ?? principal.FindFirstValue("userId");

        if (Guid.TryParse(idValue, out var parsed))
        {
            userId = parsed;
        }

        var userName = principal.FindFirstValue(ClaimTypes.Name)
                       ?? principal.FindFirstValue("name")
                       ?? principal.FindFirstValue(ClaimTypes.Email)
                       ?? principal.FindFirstValue("email");

        return (userId, userName);
    }

    private static Guid? ResolveEntityId(RouteValueDictionary routeValues)
    {
        foreach (var kv in routeValues)
        {
            if (!kv.Key.EndsWith("id", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (kv.Value is null)
            {
                continue;
            }

            if (Guid.TryParse(kv.Value.ToString(), out var id))
            {
                return id;
            }
        }

        return null;
    }

    private static string ResolveClientPlatform(HttpContext context)
    {
        var platform = context.Request.Headers["X-Client-Platform"].ToString().Trim().ToLowerInvariant();
        if (platform is "mobile" or "web")
        {
            return platform;
        }

        var userAgent = context.Request.Headers.UserAgent.ToString().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(userAgent))
        {
            return "unknown";
        }

        if (userAgent.Contains("android", StringComparison.Ordinal)
            || userAgent.Contains("iphone", StringComparison.Ordinal)
            || userAgent.Contains("okhttp", StringComparison.Ordinal)
            || userAgent.Contains("dalvik", StringComparison.Ordinal))
        {
            return "mobile";
        }

        return "web";
    }
}
