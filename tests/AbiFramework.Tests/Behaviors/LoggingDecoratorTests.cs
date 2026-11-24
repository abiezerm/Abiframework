using AbiFramework.Behaviors;
using AbiFramework.Entities;
using AbiFramework.Messaging;
using Microsoft.Extensions.Logging;

namespace AbiFramework.Tests.Behaviors;

public class LoggingDecoratorTests
{
    private readonly Mock<ILogger<LoggingDecorator.QueryHandler<TestQuery, string>>> _queryLogger;
    private readonly Mock<ILogger<LoggingDecorator.CommandHandler<TestCommand, int>>> _commandWithResponseLogger;
    private readonly Mock<ILogger<LoggingDecorator.CommandHandler<TestCommand>>> _commandLogger;

    public LoggingDecoratorTests()
    {
        _queryLogger = new Mock<ILogger<LoggingDecorator.QueryHandler<TestQuery, string>>>();
        _commandWithResponseLogger = new Mock<ILogger<LoggingDecorator.CommandHandler<TestCommand, int>>>();
        _commandLogger = new Mock<ILogger<LoggingDecorator.CommandHandler<TestCommand>>>();

        _queryLogger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        _commandWithResponseLogger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
        _commandLogger.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);
    }

    [Fact]
    public async Task QueryHandler_LogsInformation_WhenHandlingSuccessfully()
    {
        // Arrange
        var query = new TestQuery();
        var expectedResult = Result.Success("test result");
        var innerHandler = new Mock<IQueryHandler<TestQuery, string>>();
        innerHandler.Setup(h => h.Handle(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var decorator = new LoggingDecorator.QueryHandler<TestQuery, string>(
            innerHandler.Object,
            _queryLogger.Object);

        // Act
        Result<string> result = await decorator.Handle(query, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        VerifyLogInformationCalled(_queryLogger, "Handling query TestQuery");
        VerifyLogInformationContains(_queryLogger, "handled successfully");
    }

    [Fact]
    public async Task QueryHandler_LogsWarning_WhenHandlingFails()
    {
        // Arrange
        var query = new TestQuery();
        var error = DomainError.Failure("TEST.ERROR", "Test error");
        var expectedResult = Result.Failure<string>(error);
        var innerHandler = new Mock<IQueryHandler<TestQuery, string>>();
        innerHandler.Setup(h => h.Handle(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var decorator = new LoggingDecorator.QueryHandler<TestQuery, string>(
            innerHandler.Object,
            _queryLogger.Object);

        // Act
        Result<string> result = await decorator.Handle(query, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        VerifyLogWarningContains(_queryLogger, "failed");
    }

    [Fact]
    public async Task QueryHandler_LogsError_WhenExceptionIsThrown()
    {
        // Arrange
        var query = new TestQuery();
        var exception = new InvalidOperationException("Test exception");
        var innerHandler = new Mock<IQueryHandler<TestQuery, string>>();
        innerHandler.Setup(h => h.Handle(query, It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        var decorator = new LoggingDecorator.QueryHandler<TestQuery, string>(
            innerHandler.Object,
            _queryLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await decorator.Handle(query, CancellationToken.None));

        VerifyLogErrorCalled(_queryLogger, exception);
    }

    [Fact]
    public async Task CommandHandlerWithResponse_LogsInformation_WhenHandlingSuccessfully()
    {
        // Arrange
        var command = new TestCommand();
        var expectedResult = Result.Success(42);
        var innerHandler = new Mock<ICommandHandler<TestCommand, int>>();
        innerHandler.Setup(h => h.Handle(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var decorator = new LoggingDecorator.CommandHandler<TestCommand, int>(
            innerHandler.Object,
            _commandWithResponseLogger.Object);

        // Act
        Result<int> result = await decorator.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        VerifyLogInformationCalled(_commandWithResponseLogger, "Handling command TestCommand");
        VerifyLogInformationContains(_commandWithResponseLogger, "handled successfully");
    }

    [Fact]
    public async Task CommandHandlerWithResponse_LogsWarning_WhenHandlingFails()
    {
        // Arrange
        var command = new TestCommand();
        var error = DomainError.Failure("TEST.ERROR", "Test error");
        var expectedResult = Result.Failure<int>(error);
        var innerHandler = new Mock<ICommandHandler<TestCommand, int>>();
        innerHandler.Setup(h => h.Handle(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var decorator = new LoggingDecorator.CommandHandler<TestCommand, int>(
            innerHandler.Object,
            _commandWithResponseLogger.Object);

        // Act
        Result<int> result = await decorator.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        VerifyLogWarningContains(_commandWithResponseLogger, "failed");
    }

    [Fact]
    public async Task CommandHandlerWithResponse_LogsError_WhenExceptionIsThrown()
    {
        // Arrange
        var command = new TestCommand();
        var exception = new InvalidOperationException("Test exception");
        var innerHandler = new Mock<ICommandHandler<TestCommand, int>>();
        innerHandler.Setup(h => h.Handle(command, It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        var decorator = new LoggingDecorator.CommandHandler<TestCommand, int>(
            innerHandler.Object,
            _commandWithResponseLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await decorator.Handle(command, CancellationToken.None));

        VerifyLogErrorCalled(_commandWithResponseLogger, exception);
    }

    [Fact]
    public async Task CommandHandler_LogsInformation_WhenHandlingSuccessfully()
    {
        // Arrange
        var command = new TestCommand();
        var expectedResult = Result.Success();
        var innerHandler = new Mock<ICommandHandler<TestCommand>>();
        innerHandler.Setup(h => h.Handle(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var decorator = new LoggingDecorator.CommandHandler<TestCommand>(
            innerHandler.Object,
            _commandLogger.Object);

        // Act
        Result result = await decorator.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        VerifyLogInformationCalled(_commandLogger, "Handling command TestCommand");
        VerifyLogInformationContains(_commandLogger, "handled successfully");
    }

    [Fact]
    public async Task CommandHandler_LogsWarning_WhenHandlingFails()
    {
        // Arrange
        var command = new TestCommand();
        var error = DomainError.Failure("TEST.ERROR", "Test error");
        var expectedResult = Result.Failure(error);
        var innerHandler = new Mock<ICommandHandler<TestCommand>>();
        innerHandler.Setup(h => h.Handle(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var decorator = new LoggingDecorator.CommandHandler<TestCommand>(
            innerHandler.Object,
            _commandLogger.Object);

        // Act
        Result result = await decorator.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        VerifyLogWarningContains(_commandLogger, "failed");
    }

    [Fact]
    public async Task CommandHandler_LogsError_WhenExceptionIsThrown()
    {
        // Arrange
        var command = new TestCommand();
        var exception = new InvalidOperationException("Test exception");
        var innerHandler = new Mock<ICommandHandler<TestCommand>>();
        innerHandler.Setup(h => h.Handle(command, It.IsAny<CancellationToken>()))
            .ThrowsAsync(exception);

        var decorator = new LoggingDecorator.CommandHandler<TestCommand>(
            innerHandler.Object,
            _commandLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await decorator.Handle(command, CancellationToken.None));

        VerifyLogErrorCalled(_commandLogger, exception);
    }

    [Fact]
    public async Task QueryHandler_TracksExecutionTime()
    {
        // Arrange
        var query = new TestQuery();
        var expectedResult = Result.Success("test result");
        var innerHandler = new Mock<IQueryHandler<TestQuery, string>>();
        innerHandler.Setup(h => h.Handle(query, It.IsAny<CancellationToken>()))
            .Returns(async () =>
            {
                await Task.Delay(10);
                return expectedResult;
            });

        var decorator = new LoggingDecorator.QueryHandler<TestQuery, string>(
            innerHandler.Object,
            _queryLogger.Object);

        // Act
        await decorator.Handle(query, CancellationToken.None);

        // Assert
        VerifyLogInformationContains(_queryLogger, "ms");
    }

    private static void VerifyLogInformationCalled<T>(Mock<ILogger<T>> logger, string message)
    {
        logger.Invocations.Should().Contain(invocation =>
            invocation.Method.Name == nameof(ILogger.Log)
            && Equals(invocation.Arguments[0], LogLevel.Information)
            && invocation.Arguments[2].ToString()!.Contains(message));
    }

    private static void VerifyLogInformationContains<T>(Mock<ILogger<T>> logger, string contains)
    {
        logger.Invocations.Should().Contain(invocation =>
            invocation.Method.Name == nameof(ILogger.Log)
            && Equals(invocation.Arguments[0], LogLevel.Information)
            && invocation.Arguments[2].ToString()!.Contains(contains));
    }

    private static void VerifyLogWarningContains<T>(Mock<ILogger<T>> logger, string contains)
    {
        logger.Invocations.Should().Contain(invocation =>
            invocation.Method.Name == nameof(ILogger.Log)
            && Equals(invocation.Arguments[0], LogLevel.Warning)
            && invocation.Arguments[2].ToString()!.Contains(contains));
    }

    private static void VerifyLogErrorCalled<T>(Mock<ILogger<T>> logger, Exception exception)
    {
        logger.Invocations
            .Where(invocation => invocation.Method.Name == nameof(ILogger.Log)
                && Equals(invocation.Arguments[0], LogLevel.Error)
                && ReferenceEquals(invocation.Arguments[3], exception))
            .Should()
            .ContainSingle();
    }

    internal record TestQuery : IQuery<string>;
    internal record TestCommand : ICommand<int>, ICommand;
}
