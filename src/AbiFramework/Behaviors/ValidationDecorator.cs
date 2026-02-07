using AbiFramework.Entities;
using AbiFramework.Messaging;
using FluentValidation;
using Microsoft.Extensions.Logging;

namespace AbiFramework.Behaviors;

/// <summary>
/// Decorator that adds validation to command handlers using FluentValidation.
/// </summary>
public static class ValidationDecorator
{
    /// <summary>
    /// Validation decorator for command handlers that return a result.
    /// </summary>
    /// <typeparam name="TCommand">The type of the command.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    public class CommandHandler<TCommand, TResponse> : ICommandHandler<TCommand, TResponse>
        where TCommand : ICommand<TResponse>
    {
        private readonly ICommandHandler<TCommand, TResponse> _inner;
        private readonly IEnumerable<IValidator<TCommand>> _validators;
        private readonly ILogger<CommandHandler<TCommand, TResponse>> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandHandler{TCommand, TResponse}"/> class.
        /// </summary>
        /// <param name="inner">The inner command handler.</param>
        /// <param name="validators">The validators for the command.</param>
        /// <param name="logger">The logger instance.</param>
        public CommandHandler(
            ICommandHandler<TCommand, TResponse> inner,
            IEnumerable<IValidator<TCommand>> validators,
            ILogger<CommandHandler<TCommand, TResponse>> logger)
        {
            _inner = inner;
            _validators = validators;
            _logger = logger;
        }

        /// <summary>
        /// Handles the command with validation.
        /// </summary>
        /// <param name="command">The command to handle.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The result of the command execution.</returns>
        public async Task<Result<TResponse>> Handle(TCommand command, CancellationToken cancellationToken)
        {
            if (!_validators.Any())
            {
                return await _inner.Handle(command, cancellationToken);
            }

            var context = new ValidationContext<TCommand>(command);
            var validationResults = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken)));
            var failures = validationResults
                .SelectMany(r => r.Errors)
                .Where(f => f != null)
                .ToList();

            if (failures.Count != 0)
            {
                var errorMessage = string.Join("; ", failures.Select(f => f.ErrorMessage));
                var errorCode = failures.First().ErrorCode;
                _logger.LogWarning("Validation failed for command {CommandType}: {Errors}",
                    typeof(TCommand).Name, errorMessage);
                var domainError = DomainError.Validation(errorCode, errorMessage);
                return (Result<TResponse>)Result<TResponse>.Failure(new Error(domainError.Code, domainError.Description, domainError.Type));
            }

            return await _inner.Handle(command, cancellationToken);
        }
    }

    /// <summary>
    /// Validation decorator for command handlers that do not return a result.
    /// </summary>
    /// <typeparam name="TCommand">The type of the command.</typeparam>
    public class CommandHandler<TCommand> : ICommandHandler<TCommand>
        where TCommand : ICommand
    {
        private readonly ICommandHandler<TCommand> _inner;
        private readonly IEnumerable<IValidator<TCommand>> _validators;
        private readonly ILogger<CommandHandler<TCommand>> _logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandHandler{TCommand}"/> class.
        /// </summary>
        /// <param name="inner">The inner command handler.</param>
        /// <param name="validators">The validators for the command.</param>
        /// <param name="logger">The logger instance.</param>
        public CommandHandler(
            ICommandHandler<TCommand> inner,
            IEnumerable<IValidator<TCommand>> validators,
            ILogger<CommandHandler<TCommand>> logger)
        {
            _inner = inner;
            _validators = validators;
            _logger = logger;
        }

        /// <summary>
        /// Handles the command with validation.
        /// </summary>
        /// <param name="command">The command to handle.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>The result of the command execution.</returns>
        public async Task<Result> Handle(TCommand command, CancellationToken cancellationToken)
        {
            if (!_validators.Any())
            {
                return await _inner.Handle(command, cancellationToken);
            }

            var context = new ValidationContext<TCommand>(command);
            var validationResults = await Task.WhenAll(
                _validators.Select(v => v.ValidateAsync(context, cancellationToken)));
            var failures = validationResults
                .SelectMany(r => r.Errors)
                .Where(f => f != null)
                .ToList();

            if (failures.Count != 0)
            {
                var errorMessage = string.Join("; ", failures.Select(f => f.ErrorMessage));
                var errorCode = failures.First().ErrorCode;
                _logger.LogWarning("Validation failed for command {CommandType}: {Errors}",
                    typeof(TCommand).Name, errorMessage);
                var domainError = DomainError.Validation(errorCode, errorMessage);
                return Result.Failure(new Error(domainError.Code, domainError.Description, domainError.Type));
            }

            return await _inner.Handle(command, cancellationToken);
        }
    }
}
