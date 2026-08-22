using AbiFramework.Entities;
using AbiFramework.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace AbiFramework.Tests.Integration;

#pragma warning disable CS0618 // Type or member is obsolete

/// <summary>
/// Integration tests showing how Result pattern works with CustomResults for Web APIs
/// </summary>
public class ResultIntegrationTests
{
    [Fact]
    public void Result_CanBeUsedInWebApiPattern()
    {
        // Arrange
        int userId = 123;
        var result = Result.Success(userId);

        // Act
        IResult httpResult = CustomResults.Problem(result);

        // Assert
        httpResult.Should().BeOfType<Ok<int>>();
        var okResult = (Ok<int>)httpResult;
        okResult.Value.Should().Be(userId);
    }

    [Fact]
    public void FailureResult_MapsToCorrectHttpStatus()
    {
        // Arrange
        var error = Error.NotFound("USER.NOTFOUND", "User not found");
        var result = Result.Failure<int>(error);

        // Act
        IResult httpResult = CustomResults.Problem(result);

        // Assert
        ProblemHttpResult problemResult = httpResult.Should().BeOfType<ProblemHttpResult>().Subject;
        problemResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public void DomainError_CanBeConvertedToError_ForBackwardCompatibility()
    {
        // Arrange
        var domainError = DomainError.Validation("CODE", "Description");

        // Act
        var error = new Error(domainError.Code, domainError.Description, domainError.Type);
        var result = Result.Failure(error);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Type.Should().Be(ErrorType.Validation);
    }

    [Fact]
    public void Result_SupportsMatchPattern()
    {
        // Arrange
        var successResult = Result.Success(42);
        var failureResult = Result.Failure<int>(Error.Failure("CODE", "Description"));

        // Act
        int successValue = successResult.IsSuccess ? successResult.Value : 0;
        int failureValue = failureResult.IsSuccess ? failureResult.Value : 0;

        // Assert
        successValue.Should().Be(42);
        failureValue.Should().Be(0);
    }

    [Fact]
    public void ValidationError_CanBeUsedWithResult_AndCustomResults()
    {
        // Arrange
        Error[] errors = new[]
        {
            Error.Validation("FIELD1.REQUIRED", "Field 1 is required"),
            Error.Validation("FIELD2.INVALID", "Field 2 is invalid")
        };
        var validationError = new ValidationError(errors);
        var result = Result.Failure((Error)validationError);

        // Act
        IResult httpResult = CustomResults.Problem(result);

        // Assert
        ProblemHttpResult problemResult = httpResult.Should().BeOfType<ProblemHttpResult>().Subject;
        problemResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
        problemResult.ProblemDetails.Extensions["code"].Should().Be("Validation.General");
    }

    [Theory]
    [InlineData(ErrorType.NotFound, 404)]
    [InlineData(ErrorType.Validation, 400)]
    [InlineData(ErrorType.Conflict, 409)]
    [InlineData(ErrorType.Failure, 500)]
    [InlineData(ErrorType.Problem, 400)]
    public void AllErrorTypes_MapToCorrectStatusCodes(ErrorType errorType, int expectedStatusCode)
    {
        // Arrange
        var error = new Error("CODE", "Description", errorType);
        var result = Result.Failure<string>(error);

        // Act
        IResult httpResult = CustomResults.Problem(result);

        // Assert
        ProblemHttpResult problemResult = httpResult.Should().BeOfType<ProblemHttpResult>().Subject;
        problemResult.StatusCode.Should().Be(expectedStatusCode);
    }

    [Fact]
    public void Result_WithComplexBusinessObject_WorksEndToEnd()
    {
        // Arrange
        var order = new Order
        {
            Id = 1,
            CustomerName = "John Doe",
            TotalAmount = 99.99m
        };

        // Act
        var result = Result.Success(order);
        IResult httpResult = CustomResults.Problem(result);

        // Assert
        Ok<Order> okResult = httpResult.Should().BeOfType<Ok<Order>>().Subject;
        okResult.Value.Should().Be(order);
        okResult.Value!.Id.Should().Be(1);
        okResult.Value.CustomerName.Should().Be("John Doe");
    }

    [Fact]
    public void ImplicitConversion_WorksInRealWorldScenario()
    {
        // Arrange & Act
        Result<string> nullResult = GetNullableString(null);
        Result<string> valueResult = GetNullableString("test");

        // Assert
        nullResult.IsFailure.Should().BeTrue();
        nullResult.Error.Should().Be(Error.NullValue);
        valueResult.IsSuccess.Should().BeTrue();
        valueResult.Value.Should().Be("test");
    }

    [Fact]
    public void ErrorChaining_PreservesInformation()
    {
        // Arrange
        Result[] results = new[]
        {
            Result.Failure(Error.Validation("FIELD1.REQUIRED", "Field 1 required")),
            Result.Failure(Error.Validation("FIELD2.REQUIRED", "Field 2 required"))
        };

        // Act
        var validationError = ValidationError.FromResults(results);
        var finalResult = Result.Failure((Error)validationError);

        // Assert
        finalResult.IsFailure.Should().BeTrue();
        finalResult.Error.Type.Should().Be(ErrorType.Validation);
        ((ValidationError)finalResult.Error).Errors.Should().HaveCount(2);
    }

    [Fact]
    public async Task Result_HandlesAsyncOperations()
    {
        // Arrange & Act
        Task<Result<int>> task = Task.FromResult(Result.Success(42));
        Result<int> result = await task;

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void Result_CanBeUsedWithLinq()
    {
        // Arrange
        Result<int>[] results = new[]
        {
            Result.Success(1),
            Result.Success(2),
            Result.Failure<int>(Error.Failure("CODE", "Description")),
            Result.Success(3)
        };

        // Act
        var successfulResults = results.Where(r => r.IsSuccess).ToList();
        var failedResults = results.Where(r => r.IsFailure).ToList();

        // Assert
        successfulResults.Should().HaveCount(3);
        failedResults.Should().HaveCount(1);
    }

    [Fact]
    public void Result_SupportsGuardClauses()
    {
        // Arrange
        Result<User> result = GetUserById(999);

        // Act
        if (result.IsFailure)
        {
            // Early return pattern
            IResult httpResult = CustomResults.Problem(result);
            httpResult.Should().BeOfType<ProblemHttpResult>();
            return;
        }

        // This would normally continue with the success path
        Assert.Fail("Should have returned early");
    }

    private static Result<string> GetNullableString(string? value)
    {
        return value!;
    }

    private static Result<User> GetUserById(int id)
    {
        if (id == 999)
        {
            return Result.Failure<User>(Error.NotFound("USER.NOTFOUND", "User not found"));
        }

        return Result.Success(new User { Id = id, Name = "Test User" });
    }

    private class Order
    {
        public int Id { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
    }

    private class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}

#pragma warning restore CS0618 // Type or member is obsolete