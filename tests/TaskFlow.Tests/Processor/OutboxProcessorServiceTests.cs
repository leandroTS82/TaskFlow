using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Polly;
using TaskFlow.Domain.Entities;
using TaskFlow.Domain.Interfaces;
using TaskFlow.Processor.Publishing;
using TaskFlow.Processor.Services;
using TaskFlow.Processor.Settings;
using Xunit;

namespace TaskFlow.Tests.Processor;

public class OutboxProcessorServiceTests
{
    private readonly IOutboxRepository _outboxRepository;
    private readonly IMessagePublisher _publisher;
    private readonly OutboxProcessorService _service;

    public OutboxProcessorServiceTests()
    {
        _outboxRepository = Substitute.For<IOutboxRepository>();
        _publisher = Substitute.For<IMessagePublisher>();

        var services = new ServiceCollection();
        services.AddScoped<IOutboxRepository>(_ => _outboxRepository);
        var provider = services.BuildServiceProvider();
        var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();

        var settings = Options.Create(new ProcessorSettings { IntervalSeconds = 5 });
        var logger = Substitute.For<ILogger<OutboxProcessorService>>();

        _service = new OutboxProcessorService(
            scopeFactory,
            _publisher,
            ResiliencePipeline.Empty,
            settings,
            logger);
    }

    [Fact]
    public async Task RunCycle_WithPendingMessages_ShouldPublishAndMarkAsPublished()
    {
        var message = OutboxMessage.Create(Guid.NewGuid(), "SendEmail", "High");

        _outboxRepository.FindPendingAsync(Arg.Any<CancellationToken>())
                         .Returns(new[] { message });

        await _service.RunCycleAsync(CancellationToken.None);

        await _publisher.Received(1)
                        .PublishAsync(message, Arg.Any<CancellationToken>());

        await _outboxRepository.Received(1)
                               .MarkAsPublishedAsync(message.Id, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RunCycle_WithNoPendingMessages_ShouldNotPublish()
    {
        _outboxRepository.FindPendingAsync(Arg.Any<CancellationToken>())
                         .Returns(Enumerable.Empty<OutboxMessage>());

        await _service.RunCycleAsync(CancellationToken.None);

        await _publisher.DidNotReceive()
                        .PublishAsync(Arg.Any<OutboxMessage>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RunCycle_WhenPublishFails_ShouldNotMarkAsPublished()
    {
        var message = OutboxMessage.Create(Guid.NewGuid(), "SendEmail", "High");

        _outboxRepository.FindPendingAsync(Arg.Any<CancellationToken>())
                         .Returns(new[] { message });

        _publisher.PublishAsync(Arg.Any<OutboxMessage>(), Arg.Any<CancellationToken>())
                  .ThrowsAsync(new Exception("RabbitMQ unavailable"));

        await _service.RunCycleAsync(CancellationToken.None);

        await _outboxRepository.DidNotReceive()
                               .MarkAsPublishedAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task RunCycle_ShouldProcessHighPriorityBeforeLow()
    {
        var highMsg = OutboxMessage.Create(Guid.NewGuid(), "Payment", "High");
        var lowMsg = OutboxMessage.Create(Guid.NewGuid(), "Email", "Low");

        _outboxRepository.FindPendingAsync(Arg.Any<CancellationToken>())
                         .Returns(new[] { lowMsg, highMsg });

        var publishOrder = new List<Guid>();

        _publisher.PublishAsync(
            Arg.Do<OutboxMessage>(m => publishOrder.Add(m.JobId)),
            Arg.Any<CancellationToken>())
                  .Returns(Task.CompletedTask);

        await _service.RunCycleAsync(CancellationToken.None);

        publishOrder[0].Should().Be(highMsg.JobId);
        publishOrder[1].Should().Be(lowMsg.JobId);
    }
}