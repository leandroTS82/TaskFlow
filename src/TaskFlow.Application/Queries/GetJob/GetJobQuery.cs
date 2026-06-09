using Mediator;

namespace TaskFlow.Application.Queries.GetJob;

public record GetJobQuery(Guid JobId) : IRequest<JobDto>;
