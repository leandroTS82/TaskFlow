using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskFlow.Infrastructure.Extensions;
using TaskFlow.Worker.Consumers;
using TaskFlow.Worker.Settings;

namespace TaskFlow.Worker.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddWorkerServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<RabbitMqSettings>(
            configuration.GetSection(RabbitMqSettings.SectionName));

        services.AddInfrastructure(configuration);

        services.AddMassTransit(x =>
        {
            x.AddConsumer<JobCreatedConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(configuration["RabbitMQ:Host"], h =>
                {
                    h.Username(configuration["RabbitMQ:Username"]!);
                    h.Password(configuration["RabbitMQ:Password"]!);
                });

                cfg.ReceiveEndpoint("taskflow-jobs", e =>
                {
                    // retry 3x  -  to DLQ
                    e.UseMessageRetry(r => r.Intervals(
                        TimeSpan.FromSeconds(5),
                        TimeSpan.FromSeconds(15),
                        TimeSpan.FromSeconds(30)));

                    e.ConfigureConsumer<JobCreatedConsumer>(context);
                });
            });
        });

        return services;
    }
}