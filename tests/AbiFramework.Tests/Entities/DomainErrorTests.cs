using AbiFramework.Entities;

namespace AbiFramework.Tests.Entities;

public class DomainErrorTests
{
    [Fact]
    public void Constructor_SetsProperties()
    {
        // Arrange & Act
        var error = new DomainError("TEST.CODE", "Test description", ErrorType.Failure);

        // Assert
        error.Code.Should().Be("TEST.CODE");
        error.Description.Should().Be("Test description");
        error.Type.Should().Be(ErrorType.Failure);
    }

    [Fact]
    public void None_HasEmptyCodeAndDescription()
    {
        // Act
        var error = DomainError.None;

        // Assert
        error.Code.Should().BeEmpty();
        error.Description.Should().BeEmpty();
        error.Type.Should().Be(ErrorType.Failure);
    }

    [Fact]
    public void NullValue_HasPredefinedProperties()
    {
        // Act
        var error = DomainError.NullValue;

        // Assert
        error.Code.Should().Be("General.Null");
        error.Description.Should().Be("Null value was provided");
        error.Type.Should().Be(ErrorType.Failure);
    }

    [Fact]
    public void Failure_CreatesFailureError()
    {
        // Act
        var error = DomainError.Failure("FAILURE.CODE", "Failure description");

        // Assert
        error.Code.Should().Be("FAILURE.CODE");
        error.Description.Should().Be("Failure description");
        error.Type.Should().Be(ErrorType.Failure);
    }

    [Fact]
    public void NotFound_CreatesNotFoundError()
    {
        // Act
        var error = DomainError.NotFound("NOTFOUND.CODE", "Not found description");

        // Assert
        error.Code.Should().Be("NOTFOUND.CODE");
        error.Description.Should().Be("Not found description");
        error.Type.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public void Validation_CreatesValidationError()
    {
        // Act
        var error = DomainError.Validation("VALIDATION.CODE", "Validation description");

        // Assert
        error.Code.Should().Be("VALIDATION.CODE");
        error.Description.Should().Be("Validation description");
        error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void Problem_CreatesProblemError()
    {
        // Act
        var error = DomainError.Problem("PROBLEM.CODE", "Problem description");

        // Assert
        error.Code.Should().Be("PROBLEM.CODE");
        error.Description.Should().Be("Problem description");
        error.Type.Should().Be(ErrorType.Problem);
    }

    [Fact]
    public void Conflict_CreatesConflictError()
    {
        // Act
        var error = DomainError.Conflict("CONFLICT.CODE", "Conflict description");

        // Assert
        error.Code.Should().Be("CONFLICT.CODE");
        error.Description.Should().Be("Conflict description");
        error.Type.Should().Be(ErrorType.Conflict);
    }

    [Fact]
    public void RecordEquality_TwoErrorsWithSameValues_AreEqual()
    {
        // Arrange
        var error1 = DomainError.Failure("TEST.CODE", "Test description");
        var error2 = DomainError.Failure("TEST.CODE", "Test description");

        // Assert
        error1.Should().Be(error2);
    }

    [Fact]
    public void RecordEquality_TwoErrorsWithDifferentValues_AreNotEqual()
    {
        // Arrange
        var error1 = DomainError.Failure("TEST.CODE1", "Test description");
        var error2 = DomainError.Failure("TEST.CODE2", "Test description");

        // Assert
        error1.Should().NotBe(error2);
    }

    [Fact]
    public void RecordEquality_TwoErrorsWithDifferentTypes_AreNotEqual()
    {
        // Arrange
        var error1 = DomainError.Failure("TEST.CODE", "Test description");
        var error2 = DomainError.NotFound("TEST.CODE", "Test description");

        // Assert
        error1.Should().NotBe(error2);
    }

    [Fact]
    public void ToString_IncludesAllProperties()
    {
        // Arrange
        var error = DomainError.Validation("TEST.CODE", "Test description");

        // Act
        var toString = error.ToString();

        // Assert
        toString.Should().Contain("TEST.CODE");
        toString.Should().Contain("Test description");
        toString.Should().Contain("Validation");
    }

    [Fact]
    public void CanDeconstructDomainError()
    {
        // Arrange
        var error = DomainError.Failure("TEST.CODE", "Test description");

        // Act
        var (code, description, type) = error;

        // Assert
        code.Should().Be("TEST.CODE");
        description.Should().Be("Test description");
        type.Should().Be(ErrorType.Failure);
    }

    [Theory]
    [InlineData("", "")]
    [InlineData("CODE", "")]
    [InlineData("", "Description")]
    [InlineData("CODE", "Description")]
    public void Constructor_AcceptsEmptyStrings(string code, string description)
    {
        // Act
        var error = new DomainError(code, description, ErrorType.Failure);

        // Assert
        error.Code.Should().Be(code);
        error.Description.Should().Be(description);
    }

    [Fact]
    public void StaticInstances_AreSingleton()
    {
        // Act
        var none1 = DomainError.None;
        var none2 = DomainError.None;
        var nullValue1 = DomainError.NullValue;
        var nullValue2 = DomainError.NullValue;

        // Assert
        none1.Should().BeSameAs(none2);
        nullValue1.Should().BeSameAs(nullValue2);
    }

    [Fact]
    public void FactoryMethods_CreateNewInstances()
    {
        // Act
        var error1 = DomainError.Failure("CODE", "Description");
        var error2 = DomainError.Failure("CODE", "Description");

        // Assert
        error1.Should().Be(error2);
        error1.Should().NotBeSameAs(error2);
    }
}