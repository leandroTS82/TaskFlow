namespace TaskFlow.Infrastructure.Contracts;

public record JobCreatedEvent(
    Guid JobId,
    string JobType,
    string Priority);