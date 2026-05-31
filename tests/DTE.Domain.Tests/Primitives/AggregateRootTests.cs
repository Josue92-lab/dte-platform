using DTE.Domain.Primitives;
using FluentAssertions;

namespace DTE.Domain.Tests.Primitives;

public class AggregateRootTests
{
    private sealed record TestDomainEvent(Guid Id, DateTime OccurredOnUtc) : DomainEvent(Id, OccurredOnUtc);

    private sealed class TestAggregateRoot : AggregateRoot
    {
        public TestAggregateRoot(Guid id) : base(id)
        {
        }

        public void DoSomething()
        {
            RaiseDomainEvent(new TestDomainEvent(Guid.NewGuid(), DateTime.UtcNow));
        }
    }

    [Fact]
    public void RaiseDomainEvent_ShouldAddEventToCollection()
    {
        // Arrange
        var aggregateRoot = new TestAggregateRoot(Guid.NewGuid());

        // Act
        aggregateRoot.DoSomething();

        // Assert
        aggregateRoot.GetDomainEvents().Should().ContainSingle();
    }

    [Fact]
    public void ClearDomainEvents_ShouldEmptyCollection()
    {
        // Arrange
        var aggregateRoot = new TestAggregateRoot(Guid.NewGuid());
        aggregateRoot.DoSomething();

        // Act
        aggregateRoot.ClearDomainEvents();

        // Assert
        aggregateRoot.GetDomainEvents().Should().BeEmpty();
    }
}
