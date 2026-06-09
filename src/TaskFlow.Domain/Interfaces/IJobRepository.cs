using TaskFlow.Domain.Entities;

namespace TaskFlow.Domain.Interfaces;

public interface IJobRepository
{
    Task<Job?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<Job?> FindByIdempotencyKeyAsync(string key, CancellationToken ct);
    Task AddAsync(Job job, OutboxMessage outbox, CancellationToken ct);
    Task UpdateAsync(Job job, CancellationToken ct);
    Task<IEnumerable<Job>> GetAllAsync(CancellationToken ct);
}
