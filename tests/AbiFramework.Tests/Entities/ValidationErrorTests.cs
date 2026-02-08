using AbiFramework.Entities;

namespace AbiFramework.Tests.Entities;

#pragma warning disable CS0618 // Type or member is obsolete

public class ValidationErrorTests
{
    [Fact]
    public void Constructor_SetsErrorsArray()
    {
        // Arrange
        var errors = new[]
        {
            Error.Validation("VALIDATION.1", "First validation error"),
            Error.Validation("VALIDATION.2", "Second validation error")
        };

        // Act
        var validationError = new ValidationError(errors);

        // Assert
        validationError.Errors.Should().BeEquivalentTo(errors);
    }

    [Fact]
    public void Constructor_SetsValidationType()
    {
        // Arrange
        var errors = new[] { Error.Validation("CODE", "Description") };

        // Act
        var validationError = new ValidationError(errors);

        // Assert
        validationError.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void Constructor_SetsDefaultCodeAndDescription()
    {
        // Arrange
        var errors = new[] { Error.Validation("CODE", "Description") };

        // Act
        var validationError = new ValidationError(errors);

        // Assert
        validationError.Code.Should().Be("Validation.General");
        validationError.Description.Should().Be("One or more validation errors occurred");
    }

    [Fact]
    public void FromResults_CreatesValidationErrorFromFailedResults()
    {
        // Arrange
        var results = new[]
        {
            Result.Failure(Error.Validation("VALIDATION.1", "First error")),
            Result.Success(),
            Result.Failure(Error.Validation("VALIDATION.2", "Second error"))
        };

        // Act
        var validationError = ValidationError.FromResults(results);

        // Assert
        validationError.Errors.Should().HaveCount(2);
        validationError.Errors[0].Code.Should().Be("VALIDATION.1");
        validationError.Errors[1].Code.Should().Be("VALIDATION.2");
    }

    [Fact]
    public void FromResults_IgnoresSuccessfulResults()
    {
        // Arrange
        var results = new[]
        {
            Result.Success(),
            Result.Success(),
            Result.Failure(Error.Validation("VALIDATION.1", "Error"))
        };

        // Act
        var validationError = ValidationError.FromResults(results);

        // Assert
        validationError.Errors.Should().ContainSingle();
        validationError.Errors[0].Code.Should().Be("VALIDATION.1");
    }

    [Fact]
    public void FromResults_ReturnsEmptyErrorArray_WhenAllResultsSucceed()
    {
        // Arrange
        var results = new[]
        {
            Result.Success(),
            Result.Success()
        };

        // Act
        var validationError = ValidationError.FromResults(results);

        // Assert
        validationError.Errors.Should().BeEmpty();
    }

    [Fact]
    public void ValidationError_IsAnError()
    {
        // Arrange
        var errors = new[] { Error.Validation("CODE", "Description") };

        // Act
        var validationError = new ValidationError(errors);

        // Assert
        validationError.Should().BeAssignableTo<Error>();
    }

    [Fact]
    public void ValidationError_CanBeUsedInResult()
    {
        // Arrange
        var errors = new[] { Error.Validation("CODE", "Description") };
        var validationError = new ValidationError(errors);

        // Act
        var result = Result.Failure(validationError);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(validationError);
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void ValidationError_PreservesIndividualErrors()
    {
        // Arrange
        var error1 = Error.Validation("FIELD.REQUIRED", "Field is required");
        var error2 = Error.Validation("FIELD.TOOLONG", "Field is too long");
        var errors = new[] { error1, error2 };

        // Act
        var validationError = new ValidationError(errors);

        // Assert
        validationError.Errors.Should().Contain(error1);
        validationError.Errors.Should().Contain(error2);
    }

    [Fact]
    public void FromResults_WorksWithEmptyResultCollection()
    {
        // Arrange
        var results = Array.Empty<Result>();

        // Act
        var validationError = ValidationError.FromResults(results);

        // Assert
        validationError.Errors.Should().BeEmpty();
    }

    [Fact]
    public void ValidationError_SupportsRecordEquality()
    {
        // Arrange
        var errors = new[] { Error.Validation("CODE", "Description") };
        var validationError1 = new ValidationError(errors);
        var validationError2 = new ValidationError(errors);

        // Assert
        validationError1.Should().Be(validationError2);
    }

    [Fact]
    public void ValidationError_CanContainMixedErrorTypes()
    {
        // Arrange
        var errors = new[]
        {
            Error.Validation("VALIDATION.1", "Validation error"),
            Error.Failure("FAILURE.1", "Failure error"),
            Error.NotFound("NOTFOUND.1", "Not found error")
        };

        // Act
        var validationError = new ValidationError(errors);

        // Assert
        validationError.Errors.Should().HaveCount(3);
        validationError.Errors.Should().Contain(e => e.Type == ErrorType.Validation);
        validationError.Errors.Should().Contain(e => e.Type == ErrorType.Failure);
        validationError.Errors.Should().Contain(e => e.Type == ErrorType.NotFound);
    }

    [Fact]
    public void FromResults_PreservesErrorDetails()
    {
        // Arrange
        var expectedCode = "VALIDATION.CUSTOM";
        var expectedDescription = "Custom validation failed";
        var results = new[]
        {
            Result.Failure(Error.Validation(expectedCode, expectedDescription))
        };

        // Act
        var validationError = ValidationError.FromResults(results);

        // Assert
        validationError.Errors.Should().ContainSingle();
        validationError.Errors[0].Code.Should().Be(expectedCode);
        validationError.Errors[0].Description.Should().Be(expectedDescription);
    }

    [Fact]
    public void ValidationError_ToString_IncludesErrorCount()
    {
        // Arrange
        var errors = new[]
        {
            Error.Validation("CODE1", "Description1"),
            Error.Validation("CODE2", "Description2")
        };
        var validationError = new ValidationError(errors);

        // Act
        var toString = validationError.ToString();

        // Assert
        toString.Should().Contain("Validation.General");
    }

    [Fact]
    public void ValidationError_WithSingleError_StillReturnsArray()
    {
        // Arrange
        var errors = new[] { Error.Validation("CODE", "Description") };

        // Act
        var validationError = new ValidationError(errors);

        // Assert
        validationError.Errors.Should().BeOfType<Error[]>();
        validationError.Errors.Should().ContainSingle();
    }
}

#pragma warning restore CS0618 // Type or member is obsolete