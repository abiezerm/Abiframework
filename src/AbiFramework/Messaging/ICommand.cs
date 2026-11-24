namespace AbiFramework.Messaging;

public interface ICommand;

public interface ICommand<TResponse> where TResponse : notnull;
