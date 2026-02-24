using FluentValidation;
using Kickify.Application.Common.Behaviors;
using Kickify.Application.Common.Mappings;
using Microsoft.Extensions.DependencyInjection;

namespace Kickify.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            services.AddMediatR(config =>
            {
                config.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);

                config.AddOpenBehavior(typeof(LoggingPipelineBehavior<,>));
                config.AddOpenBehavior(typeof(ValidationPipelineBehavior<,>));
            });

            services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly, includeInternalTypes: true);

            // Register AutoMapper
            services.AddAutoMapper(typeof(MappingProfile).Assembly);

            return services;
        }
    }
}
