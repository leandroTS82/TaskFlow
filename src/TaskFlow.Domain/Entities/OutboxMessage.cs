namespace TaskFlow.Domain.Entities;

public class OutboxMessage
{
    private OutboxMessage() { }

    public static OutboxMessage Create(Guid jobId, string jobType, string priority)
    {
        return new OutboxMessage
        {
            Id = Guid.NewGuid(),
            JobId = jobId,
            JobType = jobType,
            Priority = priority,
            CreatedAt = DateTime.UtcNow,
            Published = false
        };
    }

    public Guid Id { get; private set; }
    public Guid JobId { get; private set; }
    public string JobType { get; private set; } = string.Empty;
    public string Priority { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public bool Published { get; private set; }
    public DateTime? PublishedAt { get; private set; }
}
