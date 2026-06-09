using MongoDB.Driver;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;
using TaskFlow.Infrastructure.Context;
using TaskFlow.Infrastructure.Mappers;

namespace TaskFlow.Infrastructure.Repositories;

public class OutboxRepository : IOutboxRepository
{
    private readonly MongoDbContext _context;

    public OutboxRepository(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<OutboxMessage>> FindPendingAsync(CancellationToken ct)
    {
        var filter = Builders<Documents.OutboxMessageDocument>.Filter.Eq(x => x.Published, false);
        var sort = Builders<Documents.OutboxMessageDocument>.Sort.Ascending(x => x.CreatedAt);
        var documents = await _context.OutboxMessages
            .Find(filter)
            .Sort(sort)
            .Limit(100)
            .ToListAsync(ct);
        return documents.Select(OutboxMessageMapper.ToDomain);
    }

    public async Task MarkAsPublishedAsync(Guid id, CancellationToken ct)
    {
        var filter = Builders<Documents.OutboxMessageDocument>.Filter.Eq(x => x.Id, id);
        var update = Builders<Documents.OutboxMessageDocument>.Update
            .Set(x => x.Published, true)
            .Set(x => x.PublishedAt, DateTime.UtcNow);
        await _context.OutboxMessages.UpdateOneAsync(filter, update, cancellationToken: ct);
    }
}
