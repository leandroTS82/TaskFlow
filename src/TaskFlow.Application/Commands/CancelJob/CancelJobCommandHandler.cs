using Mediator;
using Microsoft.Extensions.Logging;
using TaskFlow.Domain.Exceptions;
using TaskFlow.Domain.Interfaces;

namespace TaskFlow.Application.Commands.CancelJob;

public class CancelJobCommandHandler : IRequestHandler<CancelJobCommand, CancelJobResult>
{
    private readonly IJobRepository _jobRepository;
    private readonly ILogger<CancelJobCommandHandler> _logger;

    public CancelJobCommandHandler(IJobRepository jobRepository, ILogger<CancelJobCommandHandler> logger)
    {
        _jobRepository = jobRepository;
        _logger = logger;
    }

    public async ValueTask<CancelJobResult> Handle(CancelJobCommand command, CancellationToken ct)
    {
        var job = await _jobRepository.GetByIdAsync(command.JobId, ct)
            ?? throw new NotFoundException(nameof(Domain.Entities.Job), command.JobId);

        job.Cancel();

        await _jobRepository.UpdateAsync(job, ct);

        _logger.LogInformation("Job {JobId} cancelled successfully", command.JobId);

        return new CancelJobResult(job.Id);
    }
}
