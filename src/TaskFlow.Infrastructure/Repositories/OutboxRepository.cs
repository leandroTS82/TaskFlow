using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Infrastructure.Repositories;

public class OutboxRepository : IOutboxRepository
{
    public OutboxRepository()
    {
    }

    public Task<IEnumerable<OutboxMessage>> FindPendingAsync(CancellationToken ct)
    {
        return Task.FromResult<IEnumerable<OutboxMessage>>(Array.Empty<OutboxMessage>());
    }

    public Task MarkAsPublishedAsync(Guid id, CancellationToken ct)
    {
        return Task.CompletedTask;
    }
}
