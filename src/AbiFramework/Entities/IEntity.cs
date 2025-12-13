namespace AbiFramework.Entities;

public interface IEntity <TPrimaryKey>
{
    TPrimaryKey Id { get; set; }
    List<IDomainEvent> DomainEvents { get; }
    void ClearDomainEvents();
    void Raise(IDomainEvent domainEvent);
    
}
