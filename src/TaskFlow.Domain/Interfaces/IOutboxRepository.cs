using TaskFlow.Domain.Entities;

namespace TaskFlow.Domain.Interfaces;

public interface IOutboxRepository
{
    Task<IEnumerable<OutboxMessage>> FindPendingAsync(CancellationToken ct);
    Task MarkAsPublishedAsync(Guid id, CancellationToken ct);
}
