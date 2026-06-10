using FluentAssertions;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TaskFlow.Application.Commands.CreateJob;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Enums;
using TaskFlow.Domain.Interfaces;
using Xunit;

namespace TaskFlow.Tests.Application;

public class CreateJobCommandHandlerTests
{
    private readonly IJobRepository _jobRepository;
    private readonly ILogger<CreateJobCommandHandler> _logger;
    private readonly CreateJobCommandHandler _handler;

    public CreateJobCommandHandlerTests()
    {
        _jobRepository = Substitute.For<IJobRepository>();
        _logger = Substitute.For<ILogger<CreateJobCommandHandler>>();
        _handler = new CreateJobCommandHandler(_jobRepository, _logger);
    }

    [Fact]
    public async Task Handle_NewJob_ShouldCreateAndReturnJobId()
    {
        _jobRepository.FindByIdempotencyKeyAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((Job?)null);

        var command = new CreateJobCommand("email.send", Priority.High, "{}", "key-new-001");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.JobId.Should().NotBe(Guid.Empty);
        result.AlreadyExisted.Should().BeFalse();
        await _jobRepository.Received(1).AddAsync(
            Arg.Any<Job>(),
            Arg.Any<OutboxMessage>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_DuplicateIdempotencyKey_ShouldReturnExistingJobId()
    {
        var existingId = Guid.NewGuid();
        var existingJob = Job.Restore(existingId, "email.send", Priority.High, JobStatus.Pending,
            "{}", "key-dup-001", DateTime.UtcNow, null, 0, null);

        _jobRepository.FindByIdempotencyKeyAsync("key-dup-001", Arg.Any<CancellationToken>())
            .Returns(existingJob);

        var command = new CreateJobCommand("email.send", Priority.High, "{}", "key-dup-001");

        var result = await _handler.Handle(command, CancellationToken.None);

        result.JobId.Should().Be(existingId);
        result.AlreadyExisted.Should().BeTrue();
        await _jobRepository.DidNotReceive().AddAsync(
            Arg.Any<Job>(),
            Arg.Any<OutboxMessage>(),
            Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_WithScheduledAt_ShouldCreateJobWithScheduledDate()
    {
        _jobRepository.FindByIdempotencyKeyAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns((Job?)null);

        var scheduledAt = DateTime.UtcNow.AddHours(2);
        var command = new CreateJobCommand("report.generate", Priority.Low, "{}", "key-sched-001", scheduledAt);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.AlreadyExisted.Should().BeFalse();
        await _jobRepository.Received(1).AddAsync(
            Arg.Is<Job>(j => j.ScheduledAt == scheduledAt),
            Arg.Any<OutboxMessage>(),
            Arg.Any<CancellationToken>());
    }
}
