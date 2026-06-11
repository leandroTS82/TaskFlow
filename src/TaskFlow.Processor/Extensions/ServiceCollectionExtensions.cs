using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TaskFlow.Infrastructure.Extensions;
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

        services.AddHostedService<OutboxProcessorService>();

        return services;
    }
}