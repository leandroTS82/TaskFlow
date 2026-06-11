using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using TaskFlow.Domain.Interfaces;
using TaskFlow.Processor.Settings;

namespace TaskFlow.Processor.Services;

public sealed class OutboxProcessorService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ProcessorSettings _settings;
    private readonly ILogger<OutboxProcessorService> _logger;

    public OutboxProcessorService(
        IServiceScopeFactory scopeFactory,
        IOptions<ProcessorSettings> settings,
        ILogger<OutboxProcessorService> logger)
    {
        _scopeFactory = scopeFactory;
        _settings = settings.Value;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Outbox Processor started - polling every {IntervalSeconds}s",
            _settings.IntervalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            await RunCycleAsync(stoppingToken);

            await Task.Delay(
                TimeSpan.FromSeconds(_settings.IntervalSeconds),
                stoppingToken);
        }

        _logger.LogInformation("Outbox Processor stopped.");
    }

    private async Task RunCycleAsync(CancellationToken ct)
    {
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var outboxRepository = scope.ServiceProvider
                .GetRequiredService<IOutboxRepository>();

            var messages = await outboxRepository.FindPendingAsync(ct);
            var pending = messages.ToList();

            if (pending.Count == 0)
            {
                _logger.LogDebug("No pending outbox messages.");
                return;
            }

            _logger.LogInformation(
                "Found {Count} pending outbox message(s).",
                pending.Count);

            foreach (var message in pending)
            {
                _logger.LogInformation(
                    "Pending: JobId={JobId} | Type={JobType} | Priority={Priority}",
                    message.JobId,
                    message.JobType,
                    message.Priority);
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during outbox processor cycle.");
        }
    }
}