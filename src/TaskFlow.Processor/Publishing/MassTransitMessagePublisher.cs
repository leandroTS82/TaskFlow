using MassTransit;
using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Contracts;

namespace TaskFlow.Processor.Publishing;

public sealed class MassTransitMessagePublisher : IMessagePublisher
{
    private readonly IBus _bus;
    private readonly ILogger<MassTransitMessagePublisher> _logger;

    public MassTransitMessagePublisher(
        IBus bus,
        ILogger<MassTransitMessagePublisher> logger)
    {
        _bus = bus;
        _logger = logger;
    }

    public async Task PublishAsync(OutboxMessage message, CancellationToken ct)
    {
        var endpoint = await _bus.GetSendEndpoint(new Uri("queue:taskflow-jobs"));

        await endpoint.Send(
            new JobCreatedEvent(message.JobId, message.JobType, message.Priority),
            ct);

        _logger.LogDebug(
            "Published JobCreatedEvent → JobId: {JobId} | Type: {JobType} | Priority: {Priority}",
            message.JobId,
            message.JobType,
            message.Priority);
    }
}