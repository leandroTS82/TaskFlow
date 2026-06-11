using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using TaskFlow.Infrastructure.Extensions;
using TaskFlow.Processor.Publishing;
using TaskFlow.Processor.Resilience;
using TaskFlow.Processor.Services;
using TaskFlow.Processor.Settings;

namespace TaskFlow.Processor.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddProcessorServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<ProcessorSettings>(
            configuration.GetSection(ProcessorSettings.SectionName));

        services.AddInfrastructure(configuration);

        services.AddMassTransit(x =>
        {
            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(configuration["RabbitMQ:Host"], h =>
                {
                    h.Username(configuration["RabbitMQ:Username"]!);
                    h.Password(configuration["RabbitMQ:Password"]!);
                });
            });
        });

        // Singleton — becuse circuit breaker state must be shared across all publish calls
        services.AddSingleton<ResiliencePipeline>(sp =>
        {
            var logger = sp.GetRequiredService<ILoggerFactory>()
                           .CreateLogger("TaskFlow.Processor.Resilience");
            return ResiliencePipelineFactory.Create(logger);
        });

        services.AddSingleton<IMessagePublisher, MassTransitMessagePublisher>();
        services.AddHostedService<OutboxProcessorService>();

        return services;
    }
}