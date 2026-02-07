using System.Diagnostics;
using AbiFramework.Entities;
using AbiFramework.Messaging;
using Microsoft.Extensions.Logging;

namespace AbiFramework.Behaviors;

/// <summary>
/// Decorator that adds logging to query and command handlers.
/// Logs execution time and success/failure status.
/// </summary>
public static class LoggingDecorator
{
    /// <summary>
    /// Logging decorator for query handlers.
    /// </summary>
    /// <typeparam name="TQuery">The type of the query.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    public class QueryHandler<TQuery, TResponse> : IQueryHandler<TQuery, TResponse>
        where TQuery : IQuery<TResponse>
    {
        private readonly IQueryHandler<TQuery, TResponse> _inner;
        private readonly ILogger<QueryHandler<TQuery, TResponse>> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="QueryHandler{TQuery, TResponse}"/> class.
        /// </summary>
        /// <param name="inner">The inner query handler.</param>
        /// <param name="logger">The logger instance.</param>
        public QueryHandler(
            IQueryHandler<TQuery, TResponse> inner,
            ILogger<QueryHandler<TQuery, TResponse>> logger)
        {
            _inner = inner;
            _logger = logger;
        }

        /// <summary>
        /// Handles the query with logging.
        /// </summary>
        /// <param name="query">The query to handle.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The result of the query execution.</returns>
        public async Task<Result<TResponse>> Handle(TQuery query, CancellationToken cancellationToken)
        {
            var queryName = typeof(TQuery).Name;
            _logger.LogInformation("Handling query {QueryName}", queryName);

            var stopwatch = Stopwatch.StartNew();
            try
            {
                var result = await _inner.Handle(query, cancellationToken);
                stopwatch.Stop();

                if (result.IsSuccess)
                {
                    _logger.LogInformation(
                        "Query {QueryName} handled successfully in {ElapsedMilliseconds}ms",
                        queryName,
                        stopwatch.ElapsedMilliseconds);
                }
                else
                {
                    _logger.LogWarning(
                        "Query {QueryName} failed in {ElapsedMilliseconds}ms with error: {ErrorCode} - {ErrorDescription}",
                        queryName,
                        stopwatch.ElapsedMilliseconds,
                        result.Error.Code,
                        result.Error.Description);
                }

                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(
                    ex,
                    "Query {QueryName} threw an exception after {ElapsedMilliseconds}ms",
                    queryName,
                    stopwatch.ElapsedMilliseconds);
                throw;
            }
        }
    }

    /// <summary>
    /// Logging decorator for command handlers that return a result.
    /// </summary>
    /// <typeparam name="TCommand">The type of the command.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    public class CommandHandler<TCommand, TResponse> : ICommandHandler<TCommand, TResponse>
        where TCommand : ICommand<TResponse>
    {
        private readonly ICommandHandler<TCommand, TResponse> _inner;
        private readonly ILogger<CommandHandler<TCommand, TResponse>> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandHandler{TCommand, TResponse}"/> class.
        /// </summary>
        /// <param name="inner">The inner command handler.</param>
        /// <param name="logger">The logger instance.</param>
        public CommandHandler(
            ICommandHandler<TCommand, TResponse> inner,
            ILogger<CommandHandler<TCommand, TResponse>> logger)
        {
            _inner = inner;
            _logger = logger;
        }

        /// <summary>
        /// Handles the command with logging.
        /// </summary>
        /// <param name="command">The command to handle.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The result of the command execution.</returns>
        public async Task<Result<TResponse>> Handle(TCommand command, CancellationToken cancellationToken)
        {
            var commandName = typeof(TCommand).Name;
            _logger.LogInformation("Handling command {CommandName}", commandName);

            var stopwatch = Stopwatch.StartNew();
            try
            {
                var result = await _inner.Handle(command, cancellationToken);
                stopwatch.Stop();

                if (result.IsSuccess)
                {
                    _logger.LogInformation(
                        "Command {CommandName} handled successfully in {ElapsedMilliseconds}ms",
                        commandName,
                        stopwatch.ElapsedMilliseconds);
                }
                else
                {
                    _logger.LogWarning(
                        "Command {CommandName} failed in {ElapsedMilliseconds}ms with error: {ErrorCode} - {ErrorDescription}",
                        commandName,
                        stopwatch.ElapsedMilliseconds,
                        result.Error.Code,
                        result.Error.Description);
                }

                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(
                    ex,
                    "Command {CommandName} threw an exception after {ElapsedMilliseconds}ms",
                    commandName,
                    stopwatch.ElapsedMilliseconds);
                throw;
            }
        }
    }

    /// <summary>
    /// Logging decorator for command handlers that do not return a result.
    /// </summary>
    /// <typeparam name="TCommand">The type of the command.</typeparam>
    public class CommandHandler<TCommand> : ICommandHandler<TCommand>
        where TCommand : ICommand
    {
        private readonly ICommandHandler<TCommand> _inner;
        private readonly ILogger<CommandHandler<TCommand>> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandHandler{TCommand}"/> class.
        /// </summary>
        /// <param name="inner">The inner command handler.</param>
        /// <param name="logger">The logger instance.</param>
        public CommandHandler(
            ICommandHandler<TCommand> inner,
            ILogger<CommandHandler<TCommand>> logger)
        {
            _inner = inner;
            _logger = logger;
        }

        /// <summary>
        /// Handles the command with logging.
        /// </summary>
        /// <param name="command">The command to handle.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The result of the command execution.</returns>
        public async Task<Result> Handle(TCommand command, CancellationToken cancellationToken)
        {
            var commandName = typeof(TCommand).Name;
            _logger.LogInformation("Handling command {CommandName}", commandName);

            var stopwatch = Stopwatch.StartNew();
            try
            {
                var result = await _inner.Handle(command, cancellationToken);
                stopwatch.Stop();

                if (result.IsSuccess)
                {
                    _logger.LogInformation(
                        "Command {CommandName} handled successfully in {ElapsedMilliseconds}ms",
                        commandName,
                        stopwatch.ElapsedMilliseconds);
                }
                else
                {
                    _logger.LogWarning(
                        "Command {CommandName} failed in {ElapsedMilliseconds}ms with error: {ErrorCode} - {ErrorDescription}",
                        commandName,
                        stopwatch.ElapsedMilliseconds,
                        result.Error.Code,
                        result.Error.Description);
                }

                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(
                    ex,
                    "Command {CommandName} threw an exception after {ElapsedMilliseconds}ms",
                    commandName,
                    stopwatch.ElapsedMilliseconds);
                throw;
            }
        }
    }
}
