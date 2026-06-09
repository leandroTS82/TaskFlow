using Mediator;
using TaskFlow.Domain.Enums;

namespace TaskFlow.Application.Commands.CreateJob;

public record CreateJobCommand(
    string JobType,
    Priority Priority,
    string Payload,
    string IdempotencyKey,
    DateTime? ScheduledAt = null
) : IRequest<CreateJobResult>;

public record CreateJobResult(Guid JobId, bool AlreadyExisted);
