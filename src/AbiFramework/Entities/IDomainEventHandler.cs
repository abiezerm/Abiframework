using System.Threading;
using System.Threading.Tasks;

namespace AbiFramework.Entities;

public interface IDomainEventHandler<in T> where T : IDomainEvent
{
    Task Handle(T domainEvent, CancellationToken cancellationToken);
}
