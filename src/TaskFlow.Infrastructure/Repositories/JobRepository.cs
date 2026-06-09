using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Infrastructure.Repositories;

public class JobRepository : IJobRepository
{
    public JobRepository()
    {
    }

    public async Task<Job?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public async Task<Job?> FindByIdempotencyKeyAsync(string key, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public async Task AddAsync(Job job, OutboxMessage outbox, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public async Task UpdateAsync(Job job, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public async Task<IEnumerable<Job>> GetAllAsync(CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}
