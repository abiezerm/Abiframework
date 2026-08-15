using AbiFramework.Behaviors;
using AbiFramework.Entities;
using AbiFramework.Messaging;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;

namespace AbiFramework.Tests.Behaviors;

public class ValidationDecoratorTests
{
    private readonly Mock<ILogger<ValidationDecorator.CommandHandler<TestCommand, int>>> _commandWithResponseLogger;
    private readonly Mock<ILogger<ValidationDecorator.CommandHandler<TestCommand>>> _commandLogger;

    public ValidationDecoratorTests()
    {
        _commandWithResponseLogger = new Mock<ILogger<ValidationDecorator.CommandHandler<TestCommand, int>>>();
        _commandLogger = new Mock<ILogger<ValidationDecorator.CommandHandler<TestCommand>>>();
    }

    [Fact]
    public async Task CommandHandlerWithResponse_CallsInnerHandler_WhenNoValidatorsAreRegistered()
    {
        // Arrange
        var command = new TestCommand { Value = "test" };
        var expectedResult = Result.Success(42);
        var innerHandler = new Mock<ICommandHandler<TestCommand, int>>();
        innerHandler.Setup(h => h.Handle(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var validators = Enumerable.Empty<IValidator<TestCommand>>();

        var decorator = new ValidationDecorator.CommandHandler<TestCommand, int>(
            innerHandler.Object,
            validators,
            _commandWithResponseLogger.Object);

        // Act
        var result = await decorator.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        innerHandler.Verify(h => h.Handle(command, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CommandHandlerWithResponse_CallsInnerHandler_WhenValidationPasses()
    {
        // Arrange
        var command = new TestCommand { Value = "test" };
        var expectedResult = Result.Success(42);
        var innerHandler = new Mock<ICommandHandler<TestCommand, int>>();
        innerHandler.Setup(h => h.Handle(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var validator = new Mock<IValidator<TestCommand>>();
        validator.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestCommand>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var validators = new[] { validator.Object };

        var decorator = new ValidationDecorator.CommandHandler<TestCommand, int>(
            innerHandler.Object,
            validators,
            _commandWithResponseLogger.Object);

        // Act
        var result = await decorator.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        innerHandler.Verify(h => h.Handle(command, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CommandHandlerWithResponse_ReturnsFailure_WhenValidationFails()
    {
        // Arrange
        var command = new TestCommand { Value = "" };
        var innerHandler = new Mock<ICommandHandler<TestCommand, int>>();

        var validator = new Mock<IValidator<TestCommand>>();
        var validationFailure = new ValidationFailure("Value", "Value is required")
        {
            ErrorCode = "VALUE_REQUIRED"
        };
        validator.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestCommand>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new[] { validationFailure }));

        var validators = new[] { validator.Object };

        var decorator = new ValidationDecorator.CommandHandler<TestCommand, int>(
            innerHandler.Object,
            validators,
            _commandWithResponseLogger.Object);

        // Act
        var result = await decorator.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
        result.Error.Description.Should().Contain("Value is required");
        innerHandler.Verify(h => h.Handle(command, It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CommandHandlerWithResponse_LogsWarning_WhenValidationFails()
    {
        // Arrange
        var command = new TestCommand { Value = "" };
        var innerHandler = new Mock<ICommandHandler<TestCommand, int>>();

        var validator = new Mock<IValidator<TestCommand>>();
        var validationFailure = new ValidationFailure("Value", "Value is required")
        {
            ErrorCode = "VALUE_REQUIRED"
        };
        validator.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestCommand>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new[] { validationFailure }));

        var validators = new[] { validator.Object };

        var decorator = new ValidationDecorator.CommandHandler<TestCommand, int>(
            innerHandler.Object,
            validators,
            _commandWithResponseLogger.Object);

        // Act
        await decorator.Handle(command, CancellationToken.None);

        // Assert
        _commandWithResponseLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Validation failed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task CommandHandlerWithResponse_CombinesMultipleValidationErrors()
    {
        // Arrange
        var command = new TestCommand { Value = "" };
        var innerHandler = new Mock<ICommandHandler<TestCommand, int>>();

        var validator = new Mock<IValidator<TestCommand>>();
        var validationFailures = new[]
        {
            new ValidationFailure("Value", "Value is required") { ErrorCode = "VALUE_REQUIRED" },
            new ValidationFailure("Value", "Value must be at least 5 characters") { ErrorCode = "VALUE_TOO_SHORT" }
        };
        validator.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestCommand>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(validationFailures));

        var validators = new[] { validator.Object };

        var decorator = new ValidationDecorator.CommandHandler<TestCommand, int>(
            innerHandler.Object,
            validators,
            _commandWithResponseLogger.Object);

        // Act
        var result = await decorator.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Description.Should().Contain("Value is required");
        result.Error.Description.Should().Contain("Value must be at least 5 characters");
    }

    [Fact]
    public async Task CommandHandler_CallsInnerHandler_WhenNoValidatorsAreRegistered()
    {
        // Arrange
        var command = new TestCommand { Value = "test" };
        var expectedResult = Result.Success();
        var innerHandler = new Mock<ICommandHandler<TestCommand>>();
        innerHandler.Setup(h => h.Handle(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var validators = Enumerable.Empty<IValidator<TestCommand>>();

        var decorator = new ValidationDecorator.CommandHandler<TestCommand>(
            innerHandler.Object,
            validators,
            _commandLogger.Object);

        // Act
        var result = await decorator.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        innerHandler.Verify(h => h.Handle(command, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CommandHandler_CallsInnerHandler_WhenValidationPasses()
    {
        // Arrange
        var command = new TestCommand { Value = "test" };
        var expectedResult = Result.Success();
        var innerHandler = new Mock<ICommandHandler<TestCommand>>();
        innerHandler.Setup(h => h.Handle(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var validator = new Mock<IValidator<TestCommand>>();
        validator.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestCommand>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var validators = new[] { validator.Object };

        var decorator = new ValidationDecorator.CommandHandler<TestCommand>(
            innerHandler.Object,
            validators,
            _commandLogger.Object);

        // Act
        var result = await decorator.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        innerHandler.Verify(h => h.Handle(command, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CommandHandler_ReturnsFailure_WhenValidationFails()
    {
        // Arrange
        var command = new TestCommand { Value = "" };
        var innerHandler = new Mock<ICommandHandler<TestCommand>>();

        var validator = new Mock<IValidator<TestCommand>>();
        var validationFailure = new ValidationFailure("Value", "Value is required")
        {
            ErrorCode = "VALUE_REQUIRED"
        };
        validator.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestCommand>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new[] { validationFailure }));

        var validators = new[] { validator.Object };

        var decorator = new ValidationDecorator.CommandHandler<TestCommand>(
            innerHandler.Object,
            validators,
            _commandLogger.Object);

        // Act
        var result = await decorator.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
        result.Error.Description.Should().Contain("Value is required");
        innerHandler.Verify(h => h.Handle(command, It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CommandHandler_LogsWarning_WhenValidationFails()
    {
        // Arrange
        var command = new TestCommand { Value = "" };
        var innerHandler = new Mock<ICommandHandler<TestCommand>>();

        var validator = new Mock<IValidator<TestCommand>>();
        var validationFailure = new ValidationFailure("Value", "Value is required")
        {
            ErrorCode = "VALUE_REQUIRED"
        };
        validator.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestCommand>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(new[] { validationFailure }));

        var validators = new[] { validator.Object };

        var decorator = new ValidationDecorator.CommandHandler<TestCommand>(
            innerHandler.Object,
            validators,
            _commandLogger.Object);

        // Act
        await decorator.Handle(command, CancellationToken.None);

        // Assert
        _commandLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Validation failed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task CommandHandler_UsesFirstErrorCode_WhenMultipleValidationFailures()
    {
        // Arrange
        var command = new TestCommand { Value = "" };
        var innerHandler = new Mock<ICommandHandler<TestCommand>>();

        var validator = new Mock<IValidator<TestCommand>>();
        var validationFailures = new[]
        {
            new ValidationFailure("Value", "Value is required") { ErrorCode = "VALUE_REQUIRED" },
            new ValidationFailure("Value", "Value must be at least 5 characters") { ErrorCode = "VALUE_TOO_SHORT" }
        };
        validator.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestCommand>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(validationFailures));

        var validators = new[] { validator.Object };

        var decorator = new ValidationDecorator.CommandHandler<TestCommand>(
            innerHandler.Object,
            validators,
            _commandLogger.Object);

        // Act
        var result = await decorator.Handle(command, CancellationToken.None);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("VALUE_REQUIRED");
    }

    [Fact]
    public async Task CommandHandlerWithResponse_SupportsMultipleValidators()
    {
        // Arrange
        var command = new TestCommand { Value = "test" };
        var expectedResult = Result.Success(42);
        var innerHandler = new Mock<ICommandHandler<TestCommand, int>>();
        innerHandler.Setup(h => h.Handle(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        var validator1 = new Mock<IValidator<TestCommand>>();
        validator1.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestCommand>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var validator2 = new Mock<IValidator<TestCommand>>();
        validator2.Setup(v => v.ValidateAsync(It.IsAny<ValidationContext<TestCommand>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var validators = new[] { validator1.Object, validator2.Object };

        var decorator = new ValidationDecorator.CommandHandler<TestCommand, int>(
            innerHandler.Object,
            validators,
            _commandWithResponseLogger.Object);

        // Act
        var result = await decorator.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(expectedResult);
        validator1.Verify(v => v.ValidateAsync(It.IsAny<ValidationContext<TestCommand>>(), It.IsAny<CancellationToken>()), Times.Once);
        validator2.Verify(v => v.ValidateAsync(It.IsAny<ValidationContext<TestCommand>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    internal record TestCommand : ICommand<int>, ICommand
    {
        public string Value { get; init; } = string.Empty;
    }
}