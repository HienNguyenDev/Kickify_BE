using Kickify.Api.Infrastructure;
using Kickify.Api.Services;
using Kickify.Application.Abstractions.Services;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kickify.Api
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPresentation(this IServiceCollection services, IConfiguration configuration,
        IWebHostEnvironment environment)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            services.AddControllers()
                    .AddJsonOptions(opts =>
                    {
                        // Allow integer enum values (e.g. mobile clients) as well as string names.
                        opts.JsonSerializerOptions.Converters.Add(
                            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase, allowIntegerValues: true));
                        opts.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                        opts.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
                    }); ;

            services.AddExceptionHandler<GlobalExceptionHandler>();
            services.AddProblemDetails();

            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    if (environment.IsDevelopment())
                    {
                        policy
                            .SetIsOriginAllowed(_ => true)
                            .AllowAnyMethod()
                            .AllowAnyHeader()
                            .AllowCredentials();
                    }
                    else
                    {
                        var configuredOrigins = configuration
                            .GetSection("AllowedOrigins")
                            .Get<string[]>() ?? Array.Empty<string>();

                        var envOrigins = configuration["AllowedOrigins"];

                        var allowedOrigins = configuredOrigins
                            .Concat((envOrigins ?? string.Empty)
                                .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
                            .Where(origin => !string.IsNullOrWhiteSpace(origin))
                            .Distinct(StringComparer.OrdinalIgnoreCase)
                            .ToArray();

                        policy
                            .WithOrigins(allowedOrigins)
                            .AllowAnyMethod()
                            .AllowAnyHeader()
                            .AllowCredentials();
                    }
                });
            });

            services.AddScoped<IChatHubService, ChatHubService>();
            services.AddScoped<IMatchRoomHubService, MatchRoomHubService>();

            return services;
        }
    }
}
