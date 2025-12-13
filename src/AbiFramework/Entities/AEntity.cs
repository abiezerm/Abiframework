

namespace AbiFramework.Entities;

public abstract class AEntity<TPrimaryKey> : IEntity<TPrimaryKey>
{
    public virtual TPrimaryKey Id { get; set; }
    
    private readonly List<IDomainEvent> _domainEvents = [];

    public List<IDomainEvent> DomainEvents => [.. _domainEvents];

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    public void Raise(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }
}
