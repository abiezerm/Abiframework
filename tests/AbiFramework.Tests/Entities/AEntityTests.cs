using AbiFramework.Entities;

namespace AbiFramework.Tests.Entities;

public class AEntityTests
{
    [Fact]
    public void Id_CanBeSetAndRetrieved()
    {
        // Arrange & Act
        var entity = new TestEntity(42);

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
#pragma warning disable CS0618 // intentionally testing the obsolete Raise backward-compat path
        entity.Raise(domainEvent);
#pragma warning restore CS0618

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
#pragma warning disable CS0618 // intentionally testing the obsolete Raise backward-compat path
        entity1.Raise(event1);
#pragma warning restore CS0618
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
        List<IDomainEvent> events1 = entity.DomainEvents;
        List<IDomainEvent> events2 = entity.DomainEvents;

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
        List<IDomainEvent> events = entity.DomainEvents;
        events.Clear();

        // Assert
        entity.DomainEvents.Should().ContainSingle();
    }

    [Fact]
    public void CanCreateEntityWithDifferentPrimaryKeyTypes()
    {
        // Arrange & Act
        var intEntity = new IntEntity(1);
        var guidEntity = new GuidEntity(Guid.NewGuid());
        var stringEntity = new StringEntity("test");

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
        entity.PerformBusinessOperation();

        // Assert
        entity.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<TestDomainEvent>();
    }

    private class TestEntity : AEntity<int>
    {
        public TestEntity()
        {
        }

        public TestEntity(int id)
        {
            Id = id;
        }

        public void PerformBusinessOperation()
        {
#pragma warning disable CS0618 // intentionally testing the obsolete Raise backward-compat path
            Raise(new TestDomainEvent());
#pragma warning restore CS0618
        }
    }

    private class IntEntity : AEntity<int>
    {
        public IntEntity(int id)
        {
            Id = id;
        }
    }

    private class GuidEntity : AEntity<Guid>
    {
        public GuidEntity(Guid id)
        {
            Id = id;
        }
    }

    private class StringEntity : AEntity<string>
    {
        public override string Id { get; protected set; }

        public StringEntity(string id)
        {
            Id = id;
        }
    }

    private record TestDomainEvent : IDomainEvent;
}