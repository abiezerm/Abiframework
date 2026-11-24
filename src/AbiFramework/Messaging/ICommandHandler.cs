using AbiFramework.Entities;

namespace AbiFramework.Messaging;

public interface ICommandHandler<in TCommand>
    where TCommand : ICommand
{
    Task<Result> Handle(TCommand command, CancellationToken cancellationToken);
}

public interface ICommandHandler<in TCommand, TResponse>
    where TCommand : ICommand<TResponse>
    where TResponse : notnull
{
    Task<Result<TResponse>> Handle(TCommand command, CancellationToken cancellationToken);
}
