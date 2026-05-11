using Kickify.Api.Middleware;

namespace Kickify.Api.Extensions;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseRequestContextLogging(this IApplicationBuilder app)
    {
        app.UseMiddleware<RequestContextLoggingMiddleware>();

        return app;
    }

    public static IApplicationBuilder UseSystemAuditLogging(this IApplicationBuilder app)
    {
        app.UseMiddleware<SystemAuditLoggingMiddleware>();

        return app;
    }
}
