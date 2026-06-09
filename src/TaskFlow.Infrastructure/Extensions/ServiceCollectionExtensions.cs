using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using TaskFlow.Domain.Interfaces;
using TaskFlow.Infrastructure.Context;
using TaskFlow.Infrastructure.Repositories;
using TaskFlow.Infrastructure.Services;
using TaskFlow.Infrastructure.Settings;

namespace TaskFlow.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMongoDB(configuration);
        services.AddRepositories();
        services.AddHostedService<MongoDbInitializer>();
        services.AddHealthChecksConfiguration(configuration);
        return services;
    }

    private static IServiceCollection AddMongoDB(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<MongoDbSettings>(configuration.GetSection(MongoDbSettings.SectionName));

        services.AddSingleton<IMongoClient>(sp =>
        {
            var settings = configuration
                .GetSection(MongoDbSettings.SectionName)
                .Get<MongoDbSettings>()!;
            return new MongoClient(settings.ConnectionString);
        });

        services.AddSingleton<MongoDbContext>();

        return services;
    }

    private static IServiceCollection AddHealthChecksConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHealthChecks()
            .AddMongoDb(sp => sp.GetRequiredService<IMongoClient>());
        return services;
    }

    private static IServiceCollection AddRepositories(this IServiceCollection services)
    {
        services.AddScoped<IJobRepository, JobRepository>();
        services.AddScoped<IOutboxRepository, OutboxRepository>();
        return services;
    }
}
