using Mediator;
using TaskFlow.Domain.Exceptions;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.Queries.GetJob;

public class GetJobQueryHandler : IRequestHandler<GetJobQuery, JobDto>
{
    private readonly IJobRepository _jobRepository;

    public GetJobQueryHandler(IJobRepository jobRepository)
    {
        _jobRepository = jobRepository;
    }

    public async ValueTask<JobDto> Handle(GetJobQuery query, CancellationToken ct)
    {
        var job = await _jobRepository.GetByIdAsync(query.JobId, ct)
            ?? throw new NotFoundException(nameof(Domain.Entities.Job), query.JobId);

        return new JobDto(
            job.Id,
            job.JobType,
            job.Priority,
            job.Status,
            job.Payload,
            job.IdempotencyKey,
            job.CreatedAt,
            job.ScheduledAt,
            job.RetryCount,
            job.LastError);
    }
}
