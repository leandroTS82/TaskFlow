using TaskFlow.Domain.Entities;
using TaskFlow.Infrastructure.Documents;

namespace TaskFlow.Infrastructure.Mappers;

public static class OutboxMessageMapper
{
    public static OutboxMessageDocument ToDocument(OutboxMessage msg) => new()
    {
        Id = msg.Id,
        JobId = msg.JobId,
        JobType = msg.JobType,
        Priority = msg.Priority,
        CreatedAt = msg.CreatedAt,
        Published = msg.Published,
        PublishedAt = msg.PublishedAt
    };

    public static OutboxMessage ToDomain(OutboxMessageDocument doc)
    {
        return OutboxMessage.Restore(
            doc.Id,
            doc.JobId,
            doc.JobType,
            doc.Priority,
            doc.CreatedAt,
            doc.Published,
            doc.PublishedAt);
    }
}
