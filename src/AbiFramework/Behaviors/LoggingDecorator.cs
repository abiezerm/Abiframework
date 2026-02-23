using System.Diagnostics;
using AbiFramework.Entities;
using AbiFramework.Messaging;
using Microsoft.Extensions.Logging;

namespace AbiFramework.Behaviors;

public static class LoggingDecorator
{
    public class QueryHandler<TQuery, TResponse> : IQueryHandler<TQuery, TResponse>
        where TQuery : IQuery<TResponse>
    {
        private readonly IQueryHandler<TQuery, TResponse> _inner;
        private readonly ILogger<QueryHandler<TQuery, TResponse>> _logger;

        public QueryHandler(
            IQueryHandler<TQuery, TResponse> inner,
            ILogger<QueryHandler<TQuery, TResponse>> logger)
        {
            _inner = inner;
            _logger = logger;
        }

        public async Task<Result<TResponse>> Handle(TQuery query, CancellationToken cancellationToken)
        {
            var queryName = typeof(TQuery).Name;
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Handling query {QueryName}", queryName);
            }

            var stopwatch = Stopwatch.StartNew();
            try
            {
                var result = await _inner.Handle(query, cancellationToken);
                stopwatch.Stop();

                if (result.IsSuccess)
                {
                    if (_logger.IsEnabled(LogLevel.Information))
                    {
                        _logger.LogInformation(
                            "Query {QueryName} handled successfully in {ElapsedMilliseconds}ms",
                            queryName,
                            stopwatch.ElapsedMilliseconds);
                    }
                }
                else
                {
                    if (_logger.IsEnabled(LogLevel.Warning))
                    {
                        _logger.LogWarning(
                            "Query {QueryName} failed in {ElapsedMilliseconds}ms with error: {ErrorCode} - {ErrorDescription}",
                            queryName,
                            stopwatch.ElapsedMilliseconds,
                            result.Error?.Code ?? "Unknown",
                            result.Error?.Description ?? "No error details available");
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                if (_logger.IsEnabled(LogLevel.Error))
                {
                    _logger.LogError(
                        ex,
                        "Query {QueryName} threw an exception after {ElapsedMilliseconds}ms",
                        queryName,
                        stopwatch.ElapsedMilliseconds);
                }
                throw;
            }
        }
    }

    public class CommandHandler<TCommand, TResponse> : ICommandHandler<TCommand, TResponse>
        where TCommand : ICommand<TResponse>
    {
        private readonly ICommandHandler<TCommand, TResponse> _inner;
        private readonly ILogger<CommandHandler<TCommand, TResponse>> _logger;

        public CommandHandler(
            ICommandHandler<TCommand, TResponse> inner,
            ILogger<CommandHandler<TCommand, TResponse>> logger)
        {
            _inner = inner;
            _logger = logger;
        }

        public async Task<Result<TResponse>> Handle(TCommand command, CancellationToken cancellationToken)
        {
            var commandName = typeof(TCommand).Name;
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Handling command {CommandName}", commandName);
            }

            var stopwatch = Stopwatch.StartNew();
            try
            {
                var result = await _inner.Handle(command, cancellationToken);
                stopwatch.Stop();

                if (result.IsSuccess)
                {
                    if (_logger.IsEnabled(LogLevel.Information))
                    {
                        _logger.LogInformation(
                            "Command {CommandName} handled successfully in {ElapsedMilliseconds}ms",
                            commandName,
                            stopwatch.ElapsedMilliseconds);
                    }
                }
                else
                {
                    if (_logger.IsEnabled(LogLevel.Warning))
                    {
                        _logger.LogWarning(
                            "Command {CommandName} failed in {ElapsedMilliseconds}ms with error: {ErrorCode} - {ErrorDescription}",
                            commandName,
                            stopwatch.ElapsedMilliseconds,
                            result.Error?.Code ?? "Unknown",
                            result.Error?.Description ?? "No error details available");
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                if (_logger.IsEnabled(LogLevel.Error))
                {
                    _logger.LogError(
                        ex,
                        "Command {CommandName} threw an exception after {ElapsedMilliseconds}ms",
                        commandName,
                        stopwatch.ElapsedMilliseconds);
                }
                throw;
            }
        }
    }

    public class CommandHandler<TCommand> : ICommandHandler<TCommand>
        where TCommand : ICommand
    {
        private readonly ICommandHandler<TCommand> _inner;
        private readonly ILogger<CommandHandler<TCommand>> _logger;

        public CommandHandler(
            ICommandHandler<TCommand> inner,
            ILogger<CommandHandler<TCommand>> logger)
        {
            _inner = inner;
            _logger = logger;
        }

        public async Task<Result> Handle(TCommand command, CancellationToken cancellationToken)
        {
            var commandName = typeof(TCommand).Name;
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Handling command {CommandName}", commandName);
            }

            var stopwatch = Stopwatch.StartNew();
            try
            {
                var result = await _inner.Handle(command, cancellationToken);
                stopwatch.Stop();

                if (result.IsSuccess)
                {
                    if (_logger.IsEnabled(LogLevel.Information))
                    {
                        _logger.LogInformation(
                            "Command {CommandName} handled successfully in {ElapsedMilliseconds}ms",
                            commandName,
                            stopwatch.ElapsedMilliseconds);
                    }
                }
                else
                {
                    if (_logger.IsEnabled(LogLevel.Warning))
                    {
                        _logger.LogWarning(
                            "Command {CommandName} failed in {ElapsedMilliseconds}ms with error: {ErrorCode} - {ErrorDescription}",
                            commandName,
                            stopwatch.ElapsedMilliseconds,
                            result.Error?.Code ?? "Unknown",
                            result.Error?.Description ?? "No error details available");
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                if (_logger.IsEnabled(LogLevel.Error))
                {
                    _logger.LogError(
                        ex,
                        "Command {CommandName} threw an exception after {ElapsedMilliseconds}ms",
                        commandName,
                        stopwatch.ElapsedMilliseconds);
                }
                throw;
            }
        }
    }
}
