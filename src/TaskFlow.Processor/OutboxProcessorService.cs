using Microsoft.Extensions.Options;
using Polly;
using TaskFlow.Domain.Interfaces;
using TaskFlow.Processor.Publishing;
using TaskFlow.Processor.Settings;

namespace TaskFlow.Processor.Services;

public sealed class OutboxProcessorService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IMessagePublisher _publisher;
    private readonly ResiliencePipeline _resiliencePipeline;
    private readonly ProcessorSettings _settings;
    private readonly ILogger<OutboxProcessorService> _logger;

    public OutboxProcessorService(
        IServiceScopeFactory scopeFactory,
        IMessagePublisher publisher,
        ResiliencePipeline resiliencePipeline,
        IOptions<ProcessorSettings> settings,
        ILogger<OutboxProcessorService> logger)
    {
        _scopeFactory = scopeFactory;
        _publisher = publisher;
        _resiliencePipeline = resiliencePipeline;
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
            var pending = messages
                .OrderBy(m => m.Priority == "High" ? 0 : 1)
                .ThenBy(m => m.CreatedAt)
                .ToList();

            if (pending.Count == 0)
            {
                _logger.LogDebug("No pending outbox messages.");
                return;
            }

            _logger.LogInformation(
                "Processing {Count} pending outbox message(s).",
                pending.Count);

            foreach (var message in pending)
            {
                if (ct.IsCancellationRequested) break;

                try
                {
                    await _resiliencePipeline.ExecuteAsync(async token =>
                    {
                        await _publisher.PublishAsync(message, token);
                        await outboxRepository.MarkAsPublishedAsync(message.Id, token);

                        _logger.LogInformation(
                            "Published → JobId: {JobId} | Type: {JobType} | Priority: {Priority}",
                            message.JobId, message.JobType, message.Priority);
                    }, ct);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Failed to publish message for JobId: {JobId} after all retries.",
                        message.JobId);
                }
            }
        }
        catch (OperationCanceledException) when (ct.IsCancellationRequested) { }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during outbox processor cycle.");
        }
    }
}