using TaskFlow.Domain.Entities;

namespace TaskFlow.Processor.Publishing;

public interface IMessagePublisher
{
    Task PublishAsync(OutboxMessage message, CancellationToken ct);
}