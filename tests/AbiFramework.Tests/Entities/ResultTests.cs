using AbiFramework.Entities;

namespace AbiFramework.Tests.Entities;

#pragma warning disable CS0618 // Type or member is obsolete

public class ResultTests
{
    [Fact]
    public void Success_CreatesSuccessResult()
    {
        // Act
        var result = Result.Success();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().Be(Error.None);
    }

    [Fact]
    public void Success_WithValue_CreatesSuccessResultWithValue()
    {
        // Arrange
        int value = 42;

        // Act
        var result = Result.Success(value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Value.Should().Be(value);
        result.Error.Should().Be(Error.None);
    }

    [Fact]
    public void Failure_CreatesFailureResult()
    {
        // Arrange
        var error = Error.Failure("TEST.ERROR", "Test error");

        // Act
        var result = Result.Failure(error);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void Failure_WithValue_CreatesFailureResultWithoutValue()
    {
        // Arrange
        var error = Error.Failure("TEST.ERROR", "Test error");

        // Act
        var result = Result.Failure<int>(error);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void Constructor_ThrowsException_WhenSuccessWithNonNoneError()
    {
        // Act & Assert
        Func<Result> act = () => new Result(true, Error.Failure("CODE", "Description"));
        act.Should().Throw<ArgumentException>()
            .WithParameterName("error");
    }

    [Fact]
    public void Constructor_ThrowsException_WhenFailureWithNoneError()
    {
        // Act & Assert
        Func<Result> act = () => new Result(false, Error.None);
        act.Should().Throw<ArgumentException>()
            .WithParameterName("error");
    }

    [Fact]
    public void Value_ThrowsException_WhenAccessingFailureResultValue()
    {
        // Arrange
        var result = Result.Failure<int>(Error.Failure("TEST.ERROR", "Test error"));

        // Act & Assert
        Func<int> act = () => result.Value;
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("The value of a failure result can't be accessed.");
    }

    [Fact]
    public void Value_ReturnsValue_WhenAccessingSuccessResultValue()
    {
        // Arrange
        int expectedValue = 42;
        var result = Result.Success(expectedValue);

        // Act
        int value = result.Value;

        // Assert
        value.Should().Be(expectedValue);
    }

    [Fact]
    public void ImplicitConversion_FromNonNullValue_CreatesSuccessResult()
    {
        // Act
        Result<int> result = 42;

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void ImplicitConversion_FromNullValue_CreatesFailureResult()
    {
        // Act
        Result<string?> result = (string?)null;

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(Error.NullValue);
    }

    [Fact]
    public void ValidationFailure_CreatesValidationFailureResult()
    {
        // Arrange
        var error = Error.Validation("VALIDATION.ERROR", "Validation error");

        // Act
        var result = Result<int>.ValidationFailure(error);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void IsFailure_IsOppositeOfIsSuccess()
    {
        // Arrange
        var success = Result.Success();
        var failure = Result.Failure(Error.Failure("CODE", "Description"));

        // Assert
        success.IsSuccess.Should().BeTrue();
        success.IsFailure.Should().BeFalse();
        failure.IsSuccess.Should().BeFalse();
        failure.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void Result_CanBeUsedWithReferenceTypes()
    {
        // Arrange
        string value = "test string";

        // Act
        var result = Result.Success(value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(value);
    }

    [Fact]
    public void Result_CanBeUsedWithComplexTypes()
    {
        // Arrange
        var value = new TestDto { Id = 1, Name = "Test" };

        // Act
        var result = Result.Success(value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(value);
    }

    [Fact]
    public void Result_PreservesErrorInformation()
    {
        // Arrange
        var error = Error.NotFound("USER.NOTFOUND", "User with ID 42 not found");

        // Act
        var result = Result.Failure<int>(error);

        // Assert
        result.Error.Code.Should().Be("USER.NOTFOUND");
        result.Error.Description.Should().Be("User with ID 42 not found");
        result.Error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public void GenericResult_InheritsFromBaseResult()
    {
        // Arrange
        var result = Result.Success(42);

        // Assert
        result.Should().BeAssignableTo<Result>();
    }

    [Fact]
    public void Value_IsNotNull_WhenSuccessWithNonNullValue()
    {
        // Arrange
        var result = Result.Success("test");

        // Act
        string value = result.Value;

        // Assert
        value.Should().NotBeNull();
    }

    [Fact]
    public void ImplicitConversion_WorksWithNullableReferenceTypes()
    {
        // Arrange
        string? nullString = null;
        string nonNullString = "test";

        // Act
        Result<string?> nullResult = nullString;
        Result<string> nonNullResult = nonNullString;

        // Assert
        nullResult.IsFailure.Should().BeTrue();
        nonNullResult.IsSuccess.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(-1)]
    [InlineData(int.MaxValue)]
    [InlineData(int.MinValue)]
    public void Success_WorksWithDifferentValues(int value)
    {
        // Act
        var result = Result.Success(value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(value);
    }

    [Fact]
    public void Result_CanChainOperations()
    {
        // Arrange
        var result = Result.Success(42);

        // Act
        bool isEven = result.IsSuccess && result.Value % 2 == 0;

        // Assert
        isEven.Should().BeTrue();
    }

    [Fact]
    public void FailureResult_DoesNotHaveDefaultValue()
    {
        // Arrange
        var result = Result.Failure<int>(Error.Failure("CODE", "Description"));

        // Act & Assert
        Func<int> act = () => result.Value;
        act.Should().Throw<InvalidOperationException>();
    }

    private class TestDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}

#pragma warning restore CS0618 // Type or member is obsolete