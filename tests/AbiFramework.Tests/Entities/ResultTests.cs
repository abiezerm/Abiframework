using AbiFramework.Entities;

namespace AbiFramework.Tests.Entities;

public class ResultTests
{
    [Fact]
    public void Unit_ValueIsSharedInstance()
    {
        Unit first = Unit.Value;
        Unit second = Unit.Value;

        first.Should().BeSameAs(second);
    }

    [Fact]
    public void Success_CreatesSuccessResult()
    {
        // Act
        var result = Result.Success();

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().Be(DomainError.None);
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
        result.Error.Should().Be(DomainError.None);
    }

    [Fact]
    public void Failure_CreatesFailureResult()
    {
        // Arrange
        var error = DomainError.Failure("TEST.ERROR", "Test error");

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
        var error = DomainError.Failure("TEST.ERROR", "Test error");

        // Act
        var result = Result.Failure<int>(error);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void Value_ThrowsException_WhenAccessingFailureResultValue()
    {
        // Arrange
        var result = Result.Failure<int>(DomainError.Failure("TEST.ERROR", "Test error"));

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
    public void ImplicitConversion_FromError_CreatesFailureResult()
    {
        // Arrange
        var error = DomainError.Conflict("TEST.CONFLICT", "Conflict");

        // Act
        Result<int> result = ConvertToGenericResult(error);
        Result nonGenericResult = ConvertToNonGenericResult(error);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().BeSameAs(error);
        nonGenericResult.IsFailure.Should().BeTrue();
        nonGenericResult.Error.Should().BeSameAs(error);

        static Result<int> ConvertToGenericResult(DomainError value) => value;
        static Result ConvertToNonGenericResult(DomainError value) => value;
    }

    [Fact]
    public void UnionSwitch_IsExhaustiveAndImplicitConversionsWork()
    {
        // Arrange & Act
        string valueDescription = DescribeValue(42);
        string errorDescription = DescribeValue(DomainError.Conflict("TEST.CONFLICT", "Conflict"));
        string unitDescription = DescribeNonGeneric(Result.Success());
        string nonGenericErrorDescription = DescribeNonGeneric(DomainError.Failure("TEST.FAILURE", "Failure"));

        // Assert
        valueDescription.Should().Be("Value: 42");
        errorDescription.Should().Be("Error: TEST.CONFLICT");
        unitDescription.Should().Be("Success");
        nonGenericErrorDescription.Should().Be("Error: TEST.FAILURE");

        static string DescribeValue(Result<int> result) => result switch
        {
            int value => $"Value: {value}",
            DomainError error => $"Error: {error.Code}"
        };

        static string DescribeNonGeneric(Result result) => result switch
        {
            Unit => "Success",
            DomainError error => $"Error: {error.Code}"
        };
    }

    [Fact]
    public void DefaultValue_CreatesFailureWithNullValueError()
    {
        // Arrange
        Result<int> result = default;

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(DomainError.NullValue);
    }

    [Fact]
    public void ValidationFailure_CreatesValidationFailureResult()
    {
        // Arrange
        var error = DomainError.Validation("VALIDATION.ERROR", "Validation error");

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
        var failure = Result.Failure(DomainError.Failure("CODE", "Description"));

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
        var error = DomainError.NotFound("USER.NOTFOUND", "User with ID 42 not found");

        // Act
        var result = Result.Failure<int>(error);

        // Assert
        result.Error.Code.Should().Be("USER.NOTFOUND");
        result.Error.Description.Should().Be("User with ID 42 not found");
        result.Error.Type.Should().Be(ErrorType.NotFound);
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
        var result = Result.Failure<int>(DomainError.Failure("CODE", "Description"));

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
