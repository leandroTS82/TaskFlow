using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TaskFlow.Application.Commands.CancelJob;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Exceptions;
using TaskFlow.Domain.Interfaces;
using Xunit;

namespace TaskFlow.Tests.Application;

public class CancelJobCommandHandlerTests
{
    private readonly IJobRepository _jobRepository;
    private readonly ILogger<CancelJobCommandHandler> _logger;
    private readonly CancelJobCommandHandler _handler;

    public CancelJobCommandHandlerTests()
    {
        _jobRepository = Substitute.For<IJobRepository>();
        _logger = Substitute.For<ILogger<CancelJobCommandHandler>>();
        _handler = new CancelJobCommandHandler(_jobRepository, _logger);
    }

    [Fact]
    public async Task Handle_ExistingPendingJob_ShouldCancelAndUpdate()
    {
        var jobId = Guid.NewGuid();
        var job = Job.Restore(jobId, "email.send", Priority.High, JobStatus.Pending,
            "{}", "key-cancel-001", DateTime.UtcNow, null, 0, null);

        _jobRepository.GetByIdAsync(jobId, Arg.Any<CancellationToken>()).Returns(job);

        var result = await _handler.Handle(new CancelJobCommand(jobId), CancellationToken.None);

        result.JobId.Should().Be(jobId);
        await _jobRepository.Received(1).UpdateAsync(
            Arg.Is<Job>(j => j.Status == JobStatus.Cancelled),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_NonExistentJob_ShouldThrowNotFoundException()
    {
        _jobRepository.GetByIdAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns((Job?)null);

        var act = async () => await _handler.Handle(new CancelJobCommand(Guid.NewGuid()), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_CompletedJob_ShouldThrowBusinessException()
    {
        var jobId = Guid.NewGuid();
        var job = Job.Restore(jobId, "email.send", Priority.High, JobStatus.Completed,
            "{}", "key-cancel-002", DateTime.UtcNow, null, 0, null);

        _jobRepository.GetByIdAsync(jobId, Arg.Any<CancellationToken>()).Returns(job);

        var act = async () => await _handler.Handle(new CancelJobCommand(jobId), CancellationToken.None);

        await act.Should().ThrowAsync<BusinessException>();
    }
}
