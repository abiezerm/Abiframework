using AbiFramework.Entities;

namespace AbiFramework.Tests.Entities;

#pragma warning disable CS0618 // Type or member is obsolete

public class ErrorTests
{
    [Fact]
    public void Constructor_SetsProperties()
    {
        // Arrange & Act
        var error = new Error("TEST.CODE", "Test description", ErrorType.Failure);

        // Assert
        error.Code.Should().Be("TEST.CODE");
        error.Description.Should().Be("Test description");
        error.Type.Should().Be(ErrorType.Failure);
    }

    [Fact]
    public void None_HasEmptyCodeAndDescription()
    {
        // Act
        Error error = Error.None;

        // Assert
        error.Code.Should().BeEmpty();
        error.Description.Should().BeEmpty();
        error.Type.Should().Be(ErrorType.Failure);
    }

    [Fact]
    public void NullValue_HasPredefinedProperties()
    {
        // Act
        Error error = Error.NullValue;

        // Assert
        error.Code.Should().Be("General.Null");
        error.Description.Should().Be("Null value was provided");
        error.Type.Should().Be(ErrorType.Failure);
    }

    [Fact]
    public void Failure_CreatesFailureError()
    {
        // Act
        var error = Error.Failure("FAILURE.CODE", "Failure description");

        // Assert
        error.Code.Should().Be("FAILURE.CODE");
        error.Description.Should().Be("Failure description");
        error.Type.Should().Be(ErrorType.Failure);
    }

    [Fact]
    public void NotFound_CreatesNotFoundError()
    {
        // Act
        var error = Error.NotFound("NOTFOUND.CODE", "Not found description");

        // Assert
        error.Code.Should().Be("NOTFOUND.CODE");
        error.Description.Should().Be("Not found description");
        error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public void Validation_CreatesValidationError()
    {
        // Act
        var error = Error.Validation("VALIDATION.CODE", "Validation description");

        // Assert
        error.Code.Should().Be("VALIDATION.CODE");
        error.Description.Should().Be("Validation description");
        error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void Problem_CreatesProblemError()
    {
        // Act
        var error = Error.Problem("PROBLEM.CODE", "Problem description");

        // Assert
        error.Code.Should().Be("PROBLEM.CODE");
        error.Description.Should().Be("Problem description");
        error.Type.Should().Be(ErrorType.Problem);
    }

    [Fact]
    public void Conflict_CreatesConflictError()
    {
        // Act
        var error = Error.Conflict("CONFLICT.CODE", "Conflict description");

        // Assert
        error.Code.Should().Be("CONFLICT.CODE");
        error.Description.Should().Be("Conflict description");
        error.Type.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    public void RecordEquality_TwoErrorsWithSameValues_AreEqual()
    {
        // Arrange
        var error1 = Error.Failure("TEST.CODE", "Test description");
        var error2 = Error.Failure("TEST.CODE", "Test description");

        // Assert
        error1.Should().Be(error2);
    }

    [Fact]
    public void RecordEquality_TwoErrorsWithDifferentValues_AreNotEqual()
    {
        // Arrange
        var error1 = Error.Failure("TEST.CODE1", "Test description");
        var error2 = Error.Failure("TEST.CODE2", "Test description");

        // Assert
        error1.Should().NotBe(error2);
    }

    [Fact]
    public void BackwardCompatibility_ErrorAndDomainError_HaveSameStructure()
    {
        // Arrange
        var error = Error.Failure("TEST.CODE", "Test description");
        var domainError = DomainError.Failure("TEST.CODE", "Test description");

        // Assert
        error.Code.Should().Be(domainError.Code);
        error.Description.Should().Be(domainError.Description);
        error.Type.Should().Be(domainError.Type);
    }

    [Fact]
    public void StaticInstances_AreSingleton()
    {
        // Act
        Error none1 = Error.None;
        Error none2 = Error.None;
        Error nullValue1 = Error.NullValue;
        Error nullValue2 = Error.NullValue;

        // Assert
        none1.Should().BeSameAs(none2);
        nullValue1.Should().BeSameAs(nullValue2);
    }
}

#pragma warning restore CS0618 // Type or member is obsolete