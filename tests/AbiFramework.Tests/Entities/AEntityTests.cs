using AbiFramework.Entities;

namespace AbiFramework.Tests.Entities;

public class AEntityTests
{
    [Fact]
    public void Id_CanBeSetAndRetrieved()
    {
        // Arrange
        var entity = new TestEntity();

        // Act
        entity.Id = 42;

        // Assert
        entity.Id.Should().Be(42);
    }

    [Fact]
    public void DomainEvents_InitiallyEmpty()
    {
        // Arrange & Act
        var entity = new TestEntity();

        // Assert
        entity.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void AddDomainEvent_AddsEventToCollection()
    {
        // Arrange
        var entity = new TestEntity();
        var domainEvent = new TestDomainEvent();

        // Act
        entity.AddDomainEvent(domainEvent);

        // Assert
        entity.DomainEvents.Should().ContainSingle()
            .Which.Should().Be(domainEvent);
    }

    [Fact]
    public void AddDomainEvent_AddsMultipleEvents()
    {
        // Arrange
        var entity = new TestEntity();
        var event1 = new TestDomainEvent();
        var event2 = new TestDomainEvent();

        // Act
        entity.AddDomainEvent(event1);
        entity.AddDomainEvent(event2);

        // Assert
        entity.DomainEvents.Should().HaveCount(2);
        entity.DomainEvents.Should().Contain(event1);
        entity.DomainEvents.Should().Contain(event2);
    }

    [Fact]
    public void RemoveDomainEvent_RemovesEventFromCollection()
    {
        // Arrange
        var entity = new TestEntity();
        var domainEvent = new TestDomainEvent();
        entity.AddDomainEvent(domainEvent);

        // Act
        entity.RemoveDomainEvent(domainEvent);

        // Assert
        entity.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void RemoveDomainEvent_RemovesOnlySpecifiedEvent()
    {
        // Arrange
        var entity = new TestEntity();
        var event1 = new TestDomainEvent();
        var event2 = new TestDomainEvent();
        entity.AddDomainEvent(event1);
        entity.AddDomainEvent(event2);

        // Act
        entity.RemoveDomainEvent(event1);

        // Assert
        entity.DomainEvents.Should().ContainSingle()
            .Which.Should().Be(event2);
    }

    [Fact]
    public void ClearDomainEvents_RemovesAllEvents()
    {
        // Arrange
        var entity = new TestEntity();
        entity.AddDomainEvent(new TestDomainEvent());
        entity.AddDomainEvent(new TestDomainEvent());
        entity.AddDomainEvent(new TestDomainEvent());

        // Act
        entity.ClearDomainEvents();

        // Assert
        entity.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Raise_AddsEventToCollection()
    {
        // Arrange
        var entity = new TestEntity();
        var domainEvent = new TestDomainEvent();

        // Act
        entity.Raise(domainEvent);

        // Assert
        entity.DomainEvents.Should().ContainSingle()
            .Which.Should().Be(domainEvent);
    }

    [Fact]
    public void Raise_IsEquivalentToAddDomainEvent()
    {
        // Arrange
        var entity1 = new TestEntity();
        var entity2 = new TestEntity();
        var event1 = new TestDomainEvent();
        var event2 = new TestDomainEvent();

        // Act
        entity1.Raise(event1);
        entity2.AddDomainEvent(event2);

        // Assert
        entity1.DomainEvents.Should().HaveCount(1);
        entity2.DomainEvents.Should().HaveCount(1);
    }

    [Fact]
    public void DomainEvents_ReturnsNewListInstance()
    {
        // Arrange
        var entity = new TestEntity();
        var domainEvent = new TestDomainEvent();
        entity.AddDomainEvent(domainEvent);

        // Act
        var events1 = entity.DomainEvents;
        var events2 = entity.DomainEvents;

        // Assert
        events1.Should().NotBeSameAs(events2);
        events1.Should().BeEquivalentTo(events2);
    }

    [Fact]
    public void DomainEvents_ModifyingReturnedList_DoesNotAffectInternalState()
    {
        // Arrange
        var entity = new TestEntity();
        var domainEvent = new TestDomainEvent();
        entity.AddDomainEvent(domainEvent);

        // Act
        var events = entity.DomainEvents;
        events.Clear();

        // Assert
        entity.DomainEvents.Should().ContainSingle();
    }

    [Fact]
    public void CanCreateEntityWithDifferentPrimaryKeyTypes()
    {
        // Arrange & Act
        var intEntity = new IntEntity { Id = 1 };
        var guidEntity = new GuidEntity { Id = Guid.NewGuid() };
        var stringEntity = new StringEntity { Id = "test" };

        // Assert
        intEntity.Id.Should().Be(1);
        guidEntity.Id.Should().NotBeEmpty();
        stringEntity.Id.Should().Be("test");
    }

    [Fact]
    public void Entity_CanStoreComplexBusinessLogic()
    {
        // Arrange
        var entity = new TestEntity();

        // Act
        entity.PerformBusinessOperation("test");

        // Assert
        entity.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<TestDomainEvent>();
    }

    private class TestEntity : AEntity<int>
    {
        public void PerformBusinessOperation(string data)
        {
            Raise(new TestDomainEvent());
        }
    }

    private class IntEntity : AEntity<int> { }
    private class GuidEntity : AEntity<Guid> { }
    private class StringEntity : AEntity<string>
    {
        public override string Id { get; set; } = string.Empty;
    }

    private record TestDomainEvent : IDomainEvent;
}