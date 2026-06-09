using Mediator;
using TaskFlow.Application.Queries.GetJob;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.Queries.GetAllJobs;

public class GetAllJobsQueryHandler : IRequestHandler<GetAllJobsQuery, IEnumerable<JobDto>>
{
    private readonly IJobRepository _jobRepository;

    public GetAllJobsQueryHandler(IJobRepository jobRepository)
    {
        _jobRepository = jobRepository;
    }

    public async ValueTask<IEnumerable<JobDto>> Handle(GetAllJobsQuery query, CancellationToken ct)
    {
        var jobs = await _jobRepository.GetAllAsync(ct);

        return jobs.Select(job => new JobDto(
            job.Id,
            job.JobType,
            job.Priority,
            job.Status,
            job.Payload,
            job.IdempotencyKey,
            job.CreatedAt,
            job.ScheduledAt,
            job.RetryCount,
            job.LastError));
    }
}
