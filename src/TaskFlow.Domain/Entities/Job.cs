using TaskFlow.Domain.Enums;

namespace TaskFlow.Domain.Entities;

public class Job
{
    private Job() { }

    public static Job Create(
        string jobType,
        Priority priority,
        string payload,
        string idempotencyKey,
        DateTime? scheduledAt = null)
    {
        return new Job
        {
            Id = Guid.NewGuid(),
            JobType = jobType,
            Priority = priority,
            Payload = payload,
            IdempotencyKey = idempotencyKey,
            Status = JobStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            ScheduledAt = scheduledAt
        };
    }

    public Guid Id { get; private set; }
    public string JobType { get; private set; } = string.Empty;
    public Priority Priority { get; private set; }
    public JobStatus Status { get; private set; }
    public string Payload { get; private set; } = string.Empty;
    public string IdempotencyKey { get; private set; } = string.Empty;
    public DateTime CreatedAt { get; private set; }
    public DateTime? ScheduledAt { get; private set; }
}
