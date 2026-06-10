using FluentAssertions;
using TaskFlow.Domain.Entities;
using Xunit;

namespace TaskFlow.Tests.Domain;

public class OutboxMessageTests
{
    [Fact]
    public void Create_ShouldReturnUnpublishedMessage()
    {
        var jobId = Guid.NewGuid();
        var message = OutboxMessage.Create(jobId, "email.send", "High");

        message.JobId.Should().Be(jobId);
        message.JobType.Should().Be("email.send");
        message.Priority.Should().Be("High");
        message.Published.Should().BeFalse();
        message.PublishedAt.Should().BeNull();
        message.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void MarkAsPublished_ShouldSetPublishedAndTimestamp()
    {
        var message = OutboxMessage.Create(Guid.NewGuid(), "email.send", "High");

        message.MarkAsPublished();

        message.Published.Should().BeTrue();
        message.PublishedAt.Should().NotBeNull();
        message.PublishedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }
}
