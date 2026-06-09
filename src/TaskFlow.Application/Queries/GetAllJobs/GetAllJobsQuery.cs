using Mediator;
using TaskFlow.Application.Queries.GetJob;

namespace TaskFlow.Application.Queries.GetAllJobs;

public record GetAllJobsQuery : IRequest<IEnumerable<JobDto>>;
