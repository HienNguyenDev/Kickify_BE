using Kickify.Api.Infrastructure;
using Kickify.Api.Services;
using Kickify.Application.Abstractions.Services;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Kickify.Api
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddPresentation(this IServiceCollection services)
        {
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            services.AddControllers()
                    .AddJsonOptions(opts =>
                    {
                        opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
                        opts.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
                        opts.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.Never;
                    }); ;

            services.AddExceptionHandler<GlobalExceptionHandler>();
            services.AddProblemDetails();

            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });
            services.AddScoped<IChatHubService, ChatHubService>();

            return services;
        }
    }
}
