using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;
using TaskFlow.Infrastructure.Context;
using TaskFlow.Infrastructure.Documents;

namespace TaskFlow.Infrastructure.Services;

public class MongoDbInitializer : IHostedService
{
    private readonly MongoDbContext _context;
    private readonly ILogger<MongoDbInitializer> _logger;

    public MongoDbInitializer(MongoDbContext context, ILogger<MongoDbInitializer> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Initializing MongoDB indexes...");

        try
        {
            await CreateJobIndexesAsync(cancellationToken);
            await CreateOutboxIndexesAsync(cancellationToken);
            _logger.LogInformation("MongoDB indexes ready");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to initialize MongoDB indexes. The application will continue, but indexes may be missing.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private async Task CreateJobIndexesAsync(CancellationToken ct)
    {
        var indexes = new[]
        {
            new CreateIndexModel<JobDocument>(
                Builders<JobDocument>.IndexKeys.Ascending(x => x.IdempotencyKey),
                new CreateIndexOptions { Unique = true, Name = "idx_jobs_idempotency_key" }),

            new CreateIndexModel<JobDocument>(
                Builders<JobDocument>.IndexKeys
                    .Ascending(x => x.Status)
                    .Descending(x => x.Priority)
                    .Ascending(x => x.CreatedAt),
                new CreateIndexOptions { Name = "idx_jobs_status_priority_created" }),

            new CreateIndexModel<JobDocument>(
                Builders<JobDocument>.IndexKeys
                    .Ascending(x => x.ScheduledAt)
                    .Ascending(x => x.Status),
                new CreateIndexOptions { Name = "idx_jobs_scheduled_status" })
        };

        await _context.Jobs.Indexes.CreateManyAsync(indexes, new CreateManyIndexesOptions(), ct);
    }

    private async Task CreateOutboxIndexesAsync(CancellationToken ct)
    {
        var index = new CreateIndexModel<OutboxMessageDocument>(
            Builders<OutboxMessageDocument>.IndexKeys
                .Ascending(x => x.Published)
                .Ascending(x => x.CreatedAt),
            new CreateIndexOptions { Name = "idx_outbox_published_created" });

        await _context.OutboxMessages.Indexes.CreateOneAsync(index, (CreateOneIndexOptions?)null, ct);
    }
}
