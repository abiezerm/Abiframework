using AbiFramework.Entities;

namespace AbiFramework.Tests.Entities;

public class ErrorTypeTests
{
    [Fact]
    public void ErrorType_HasExpectedValues()
    {
        // Assert
        Enum.IsDefined(ErrorType.Failure).Should().BeTrue();
        Enum.IsDefined(ErrorType.Validation).Should().BeTrue();
        Enum.IsDefined(ErrorType.Problem).Should().BeTrue();
        Enum.IsDefined(ErrorType.NotFound).Should().BeTrue();
        Enum.IsDefined(ErrorType.Conflict).Should().BeTrue();
    }

    [Fact]
    public void ErrorType_HasExpectedNumericValues()
    {
        // Assert
        ((int)ErrorType.Failure).Should().Be(0);
        ((int)ErrorType.Validation).Should().Be(1);
        ((int)ErrorType.Problem).Should().Be(2);
        ((int)ErrorType.NotFound).Should().Be(3);
        ((int)ErrorType.Conflict).Should().Be(4);
    }

    [Theory]
    [InlineData(ErrorType.Failure)]
    [InlineData(ErrorType.Validation)]
    [InlineData(ErrorType.Problem)]
    [InlineData(ErrorType.NotFound)]
    [InlineData(ErrorType.Conflict)]
    public void ErrorType_CanBeUsedInSwitchExpression(ErrorType errorType)
    {
        // Act
        string description = errorType switch
        {
            ErrorType.Failure => "Failure",
            ErrorType.Validation => "Validation",
            ErrorType.Problem => "Problem",
            ErrorType.NotFound => "NotFound",
            ErrorType.Conflict => "Conflict",
            _ => "Unknown"
        };

        // Assert
        description.Should().NotBe("Unknown");
    }

    [Fact]
    public void ErrorType_CanBeConvertedToString()
    {
        // Act
        string failureName = ErrorType.Failure.ToString();
        string validationName = ErrorType.Validation.ToString();

        // Assert
        failureName.Should().Be("Failure");
        validationName.Should().Be("Validation");
    }

    [Fact]
    public void ErrorType_CanBeParsedFromString()
    {
        // Act
        ErrorType parsed = Enum.Parse<ErrorType>("Validation");

        // Assert
        parsed.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void ErrorType_HasAllExpectedMembers()
    {
        // Act
        ErrorType[] allValues = Enum.GetValues<ErrorType>();

        // Assert
        allValues.Should().HaveCount(5);
        allValues.Should().Contain(ErrorType.Failure);
        allValues.Should().Contain(ErrorType.Validation);
        allValues.Should().Contain(ErrorType.Problem);
        allValues.Should().Contain(ErrorType.NotFound);
        allValues.Should().Contain(ErrorType.Conflict);
    }

    [Fact]
    public void ErrorType_SupportsComparison()
    {
        // Assert
        ErrorType.Failure.Should().Be(ErrorType.Failure);
        ErrorType.Validation.Should().NotBe(ErrorType.Failure);
        (ErrorType.Failure < ErrorType.Validation).Should().BeTrue();
    }

    [Fact]
    public void ErrorType_CanBeUsedInDictionary()
    {
        // Arrange
        var errorMessages = new Dictionary<ErrorType, string>
        {
            { ErrorType.Failure, "A failure occurred" },
            { ErrorType.Validation, "Validation failed" },
            { ErrorType.NotFound, "Resource not found" }
        };

        // Act & Assert
        errorMessages[ErrorType.Failure].Should().Be("A failure occurred");
        errorMessages[ErrorType.Validation].Should().Be("Validation failed");
        errorMessages.ContainsKey(ErrorType.Conflict).Should().BeFalse();
    }

    [Fact]
    public void ErrorType_GetNames_ReturnsAllNames()
    {
        // Act
        string[] names = Enum.GetNames<ErrorType>();

        // Assert
        names.Should().Contain("Failure");
        names.Should().Contain("Validation");
        names.Should().Contain("Problem");
        names.Should().Contain("NotFound");
        names.Should().Contain("Conflict");
    }
}