using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Infrastructure.Documents;

namespace TaskFlow.Infrastructure.Mappers;

public static class JobMapper
{
    public static JobDocument ToDocument(Job job) => new()
    {
        Id = job.Id,
        JobType = job.JobType,
        Priority = job.Priority.ToString(),
        Status = job.Status.ToString(),
        Payload = job.Payload,
        IdempotencyKey = job.IdempotencyKey,
        CreatedAt = job.CreatedAt,
        ScheduledAt = job.ScheduledAt,
        RetryCount = job.RetryCount,
        LastError = job.LastError
    };

    public static Job ToDomain(JobDocument doc)
    {
        var priority = Enum.Parse<Priority>(doc.Priority);
        var status = Enum.Parse<JobStatus>(doc.Status);

        return Job.Restore(
            doc.Id,
            doc.JobType,
            priority,
            status,
            doc.Payload,
            doc.IdempotencyKey,
            doc.CreatedAt,
            doc.ScheduledAt,
            doc.RetryCount,
            doc.LastError);
    }
}
