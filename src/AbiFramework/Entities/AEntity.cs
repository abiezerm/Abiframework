

namespace AbiFramework.Entities;

/// <summary>
/// Abstract base class for entities with domain event support.
/// </summary>
/// <typeparam name="TPrimaryKey">The type of the entity's primary key.</typeparam>
public abstract class AEntity<TPrimaryKey> : IEntity<TPrimaryKey>
{
    /// <summary>
    /// Gets or sets the entity's primary key.
    /// </summary>
    public virtual TPrimaryKey Id { get; set; }

    private readonly List<IDomainEvent> _domainEvents = [];

    /// <summary>
    /// Gets the collection of domain events raised by this entity.
    /// </summary>
    public List<IDomainEvent> DomainEvents => [.. _domainEvents];

    /// <summary>
    /// Clears all domain events from this entity.
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    /// <summary>
    /// Adds a domain event to this entity.
    /// </summary>
    /// <param name="domainEvent">The domain event to add.</param>
    public void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Removes a domain event from this entity.
    /// </summary>
    /// <param name="domainEvent">The domain event to remove.</param>
    public void RemoveDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Remove(domainEvent);
    }

    /// <summary>
    /// Raises a domain event by adding it to the entity's domain events collection.
    /// This method is maintained for backward compatibility and calls AddDomainEvent internally.
    /// </summary>
    /// <param name="domainEvent">The domain event to raise.</param>
    [Obsolete("Use AddDomainEvent instead for clarity", false)]
    public void Raise(IDomainEvent domainEvent)
    {
        AddDomainEvent(domainEvent);
    }
}
