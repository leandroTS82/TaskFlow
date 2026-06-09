using MongoDB.Driver;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Interfaces;
using TaskFlow.Infrastructure.Context;
using TaskFlow.Infrastructure.Mappers;

namespace TaskFlow.Infrastructure.Repositories;

public class JobRepository : IJobRepository
{
    private readonly MongoDbContext _context;

    public JobRepository(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<Job?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        var filter = Builders<Documents.JobDocument>.Filter.Eq(x => x.Id, id);
        var document = await _context.Jobs.Find(filter).FirstOrDefaultAsync(ct);
        return document is null ? null : JobMapper.ToDomain(document);
    }

    public async Task<Job?> FindByIdempotencyKeyAsync(string key, CancellationToken ct)
    {
        var filter = Builders<Documents.JobDocument>.Filter.Eq(x => x.IdempotencyKey, key);
        var document = await _context.Jobs.Find(filter).FirstOrDefaultAsync(ct);
        return document is null ? null : JobMapper.ToDomain(document);
    }

    public async Task AddAsync(Job job, OutboxMessage outbox, CancellationToken ct)
    {
        var jobDocument = JobMapper.ToDocument(job);
        var outboxDocument = OutboxMessageMapper.ToDocument(outbox);

        using var session = await _context.Client.StartSessionAsync(cancellationToken: ct);
        session.StartTransaction();

        try
        {
            await _context.Jobs.InsertOneAsync(session, jobDocument, cancellationToken: ct);
            await _context.OutboxMessages.InsertOneAsync(session, outboxDocument, cancellationToken: ct);
            await session.CommitTransactionAsync(ct);
        }
        catch
        {
            await session.AbortTransactionAsync(ct);
            throw;
        }
    }

    public async Task UpdateAsync(Job job, CancellationToken ct)
    {
        var document = JobMapper.ToDocument(job);
        var filter = Builders<Documents.JobDocument>.Filter.Eq(x => x.Id, document.Id);
        await _context.Jobs.ReplaceOneAsync(filter, document, cancellationToken: ct);
    }

    public async Task<IEnumerable<Job>> GetAllAsync(CancellationToken ct)
    {
        var documents = await _context.Jobs.Find(Builders<Documents.JobDocument>.Filter.Empty).ToListAsync(ct);
        return documents.Select(JobMapper.ToDomain);
    }
}
