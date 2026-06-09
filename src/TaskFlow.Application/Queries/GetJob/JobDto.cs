using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.Queries.GetJob;

public record JobDto(
    Guid Id,
    string JobType,
    Priority Priority,
    JobStatus Status,
    string Payload,
    string IdempotencyKey,
    DateTime CreatedAt,
    DateTime? ScheduledAt,
    int RetryCount,
    string? LastError
);
