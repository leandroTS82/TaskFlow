using Mediator;
using Microsoft.Extensions.Logging;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.Commands.CreateJob;

public class CreateJobCommandHandler : IRequestHandler<CreateJobCommand, CreateJobResult>
{
    private readonly IJobRepository _jobRepository;
    private readonly ILogger<CreateJobCommandHandler> _logger;

    public CreateJobCommandHandler(IJobRepository jobRepository, ILogger<CreateJobCommandHandler> logger)
    {
        _jobRepository = jobRepository;
        _logger = logger;
    }

    public async ValueTask<CreateJobResult> Handle(CreateJobCommand command, CancellationToken ct)
    {
        var existing = await _jobRepository.FindByIdempotencyKeyAsync(command.IdempotencyKey, ct);
        if (existing is not null)
        {
            _logger.LogInformation("Idempotent request detected for key {Key}. Returning existing job {JobId}",
                command.IdempotencyKey, existing.Id);
            return new CreateJobResult(existing.Id, AlreadyExisted: true);
        }

        var job = Job.Create(
            command.JobType,
            command.Priority,
            command.Payload,
            command.IdempotencyKey,
            command.ScheduledAt);

        var outbox = OutboxMessage.Create(job.Id, job.JobType, job.Priority.ToString());

        await _jobRepository.AddAsync(job, outbox, ct);

        _logger.LogInformation("Job {JobId} created with type {JobType} and priority {Priority}",
            job.Id, job.JobType, job.Priority);

        return new CreateJobResult(job.Id, AlreadyExisted: false);
    }
}
