using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Exceptions;

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
            ScheduledAt = scheduledAt,
            RetryCount = 0
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
    public int RetryCount { get; private set; }
    public string? LastError { get; private set; }

    public void Cancel()
    {
        if (Status != JobStatus.Pending && Status != JobStatus.Running)
            throw new BusinessException($"Only Pending or Running jobs can be cancelled. Current status: {Status}.");
        Status = JobStatus.Cancelled;
    }

    public void MarkAsRunning()
    {
        if (Status != JobStatus.Pending)
            throw new BusinessException($"Only Pending jobs can be marked as Running. Current status: {Status}.");
        Status = JobStatus.Running;
    }

    public void MarkAsCompleted()
    {
        if (Status != JobStatus.Running)
            throw new BusinessException($"Only Running jobs can be marked as Completed. Current status: {Status}.");
        Status = JobStatus.Completed;
    }

    public void MarkAsFailed(string? error = null)
    {
        if (Status != JobStatus.Running)
            throw new BusinessException($"Only Running jobs can be marked as Failed. Current status: {Status}.");
        Status = JobStatus.Failed;
        LastError = error;
        RetryCount++;
    }

    public static Job Restore(
        Guid id,
        string jobType,
        Priority priority,
        JobStatus status,
        string payload,
        string idempotencyKey,
        DateTime createdAt,
        DateTime? scheduledAt,
        int retryCount,
        string? lastError)
    {
        return new Job
        {
            Id = id,
            JobType = jobType,
            Priority = priority,
            Status = status,
            Payload = payload,
            IdempotencyKey = idempotencyKey,
            CreatedAt = createdAt,
            ScheduledAt = scheduledAt,
            RetryCount = retryCount,
            LastError = lastError
        };
    }
}
