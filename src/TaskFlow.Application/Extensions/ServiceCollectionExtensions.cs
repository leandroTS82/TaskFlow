using FluentValidation;
using Mediator;
using Microsoft.Extensions.DependencyInjection;
using TaskFlow.Application.Behaviors;

namespace TaskFlow.Application.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediator(options =>
        {
            options.ServiceLifetime = ServiceLifetime.Scoped;
        });

        services.AddValidatorsFromAssembly(
            typeof(ServiceCollectionExtensions).Assembly,
            includeInternalTypes: true);

        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        return services;
    }
}
