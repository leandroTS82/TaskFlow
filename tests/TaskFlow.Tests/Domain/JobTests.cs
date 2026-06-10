using FluentAssertions;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Exceptions;
using TaskFlow.Tests.Helpers;
using Xunit;

namespace TaskFlow.Tests.Domain;

public class JobTests
{
    [Fact]
    public void Create_ShouldReturnJobWithPendingStatus()
    {
        var job = JobFactory.CreatePending("key-001");

        job.Status.Should().Be(JobStatus.Pending);
        job.Id.Should().NotBe(Guid.Empty);
        job.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_ShouldAssignPropertiesCorrectly()
    {
        var scheduled = DateTime.UtcNow.AddHours(1);
        var job = Job.Create("report.generate", Priority.Low, "{\"id\":1}", "key-002", scheduled);

        job.JobType.Should().Be("report.generate");
        job.Priority.Should().Be(Priority.Low);
        job.Payload.Should().Be("{\"id\":1}");
        job.IdempotencyKey.Should().Be("key-002");
        job.ScheduledAt.Should().Be(scheduled);
        job.RetryCount.Should().Be(0);
    }

    [Fact]
    public void MarkAsRunning_FromPending_ShouldSucceed()
    {
        var job = JobFactory.CreatePending("key-003");

        job.MarkAsRunning();

        job.Status.Should().Be(JobStatus.Running);
    }

    [Fact]
    public void MarkAsRunning_FromRunning_ShouldThrowBusinessException()
    {
        var job = JobFactory.CreatePending("key-004");
        job.MarkAsRunning();

        var act = () => job.MarkAsRunning();

        act.Should().Throw<BusinessException>();
    }

    [Fact]
    public void MarkAsCompleted_FromRunning_ShouldSucceed()
    {
        var job = JobFactory.CreatePending("key-005");
        job.MarkAsRunning();

        job.MarkAsCompleted();

        job.Status.Should().Be(JobStatus.Completed);
    }

    [Fact]
    public void MarkAsCompleted_FromPending_ShouldThrowBusinessException()
    {
        var job = JobFactory.CreatePending("key-006");

        var act = () => job.MarkAsCompleted();

        act.Should().Throw<BusinessException>();
    }

    [Fact]
    public void Cancel_FromPending_ShouldSucceed()
    {
        var job = JobFactory.CreatePending("key-007");

        job.Cancel();

        job.Status.Should().Be(JobStatus.Cancelled);
    }

    [Fact]
    public void Cancel_FromRunning_ShouldSucceed()
    {
        var job = JobFactory.CreateRunning("key-008");

        job.Cancel();

        job.Status.Should().Be(JobStatus.Cancelled);
    }

    [Fact]
    public void Cancel_FromCompleted_ShouldThrowBusinessException()
    {
        var job = JobFactory.CreateRunning("key-009");

        job.MarkAsCompleted();

        var act = () => job.Cancel();

        act.Should().Throw<BusinessException>()
            .WithMessage("*Completed*");
    }

    [Fact]
    public void Cancel_FromCancelled_ShouldThrowBusinessException()
    {
        var job = JobFactory.CreateRunning("key-010");
        job.Cancel();

        var act = () => job.Cancel();

        act.Should().Throw<BusinessException>();
    }

    [Fact]
    public void MarkAsFailed_FromRunning_ShouldIncrementRetryCount()
    {
        var job = JobFactory.CreateRunning("key-011");

        job.MarkAsFailed("Timeout error");

        job.Status.Should().Be(JobStatus.Failed);
        job.RetryCount.Should().Be(1);
        job.LastError.Should().Be("Timeout error");
    }

    [Fact]
    public void MarkAsFailed_FromPending_ShouldThrowBusinessException()
    {
        var job = JobFactory.CreatePending("key-012");

        var act = () => job.MarkAsFailed();

        act.Should().Throw<BusinessException>();
    }

    [Fact]
    public void Restore_ShouldReconstructJobWithGivenState()
    {
        var id = Guid.NewGuid();
        var createdAt = DateTime.UtcNow.AddMinutes(-10);

        var job = Job.Restore(id, "email.send", Priority.High, JobStatus.Running,
            "{}", "key-013", createdAt, null,  0, null);

        job.Id.Should().Be(id);
        job.Status.Should().Be(JobStatus.Running);
        job.CreatedAt.Should().Be(createdAt);
    }
}
