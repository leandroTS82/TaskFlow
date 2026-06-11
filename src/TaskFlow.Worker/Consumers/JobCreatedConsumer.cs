using MassTransit;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Interfaces;
using TaskFlow.Infrastructure.Contracts;

namespace TaskFlow.Worker.Consumers;

public sealed class JobCreatedConsumer : IConsumer<JobCreatedEvent>
{
    private readonly IJobRepository _jobRepository;
    private readonly ILogger<JobCreatedConsumer> _logger;

    public JobCreatedConsumer(
        IJobRepository jobRepository,
        ILogger<JobCreatedConsumer> logger)
    {
        _jobRepository = jobRepository;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<JobCreatedEvent> context)
    {
        var message = context.Message;
        var ct = context.CancellationToken;

        _logger.LogInformation(
            "Consuming JobCreatedEvent — JobId: {JobId} | Type: {JobType} | Priority: {Priority}",
            message.JobId, message.JobType, message.Priority);

        var job = await _jobRepository.GetByIdAsync(message.JobId, ct);

        if (job is null)
        {
            _logger.LogWarning("Job {JobId} not found — skipping.", message.JobId);
            return;
        }

        if (job.Status != JobStatus.Pending)
        {
            _logger.LogInformation(
                "Job {JobId} already in status {Status} — skipping.",
                message.JobId, job.Status);
            return;
        }

        job.MarkAsRunning();
        await _jobRepository.UpdateAsync(job, ct);

        try
        {
            _logger.LogInformation(
                "Processing job {JobId} of type {JobType}...",
                job.Id, job.JobType);

            await Task.Delay(100, ct);

            job.MarkAsCompleted();
            await _jobRepository.UpdateAsync(job, ct);

            _logger.LogInformation("Job {JobId} completed successfully.", job.Id);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Job {JobId} was cancelled during processing.", job.Id);
            job.Cancel();
            await _jobRepository.UpdateAsync(job, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Job {JobId} failed during processing.", job.Id);
            job.MarkAsFailed(ex.Message);
            await _jobRepository.UpdateAsync(job, ct);
            throw;
        }
    }
}