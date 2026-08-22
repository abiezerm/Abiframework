using AbiFramework.Entities;
using AbiFramework.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace AbiFramework.Tests.Web;

#pragma warning disable CS0618 // Type or member is obsolete

public class CustomResultsTests
{
    [Fact]
    public void Problem_ReturnsOk_WhenResultIsSuccess()
    {
        // Arrange
        var result = Result.Success();

        // Act
        IResult httpResult = CustomResults.Problem(result);

        // Assert
        httpResult.Should().BeOfType<Ok>();
    }

    [Fact]
    public void Problem_WithValue_ReturnsOkWithValue_WhenResultIsSuccess()
    {
        // Arrange
        int value = 42;
        var result = Result.Success(value);

        // Act
        IResult httpResult = CustomResults.Problem(result);

        // Assert
        Ok<int> okResult = httpResult.Should().BeOfType<Ok<int>>().Subject;
        okResult.Value.Should().Be(value);
    }

    [Fact]
    public void Problem_ReturnsProblemDetails_WhenResultIsFailure()
    {
        // Arrange
        var error = Error.Failure("TEST.ERROR", "Test error description");
        var result = Result.Failure(error);

        // Act
        IResult httpResult = CustomResults.Problem(result);

        // Assert
        httpResult.Should().BeOfType<ProblemHttpResult>();
    }

    [Fact]
    public void Problem_Returns404_WhenErrorTypeIsNotFound()
    {
        // Arrange
        var error = Error.NotFound("ENTITY.NOTFOUND", "Entity not found");
        var result = Result.Failure(error);

        // Act
        IResult httpResult = CustomResults.Problem(result);

        // Assert
        ProblemHttpResult problemResult = httpResult.Should().BeOfType<ProblemHttpResult>().Subject;
        problemResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public void Problem_Returns400_WhenErrorTypeIsValidation()
    {
        // Arrange
        var error = Error.Validation("VALIDATION.ERROR", "Validation failed");
        var result = Result.Failure(error);

        // Act
        IResult httpResult = CustomResults.Problem(result);

        // Assert
        ProblemHttpResult problemResult = httpResult.Should().BeOfType<ProblemHttpResult>().Subject;
        problemResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public void Problem_Returns409_WhenErrorTypeIsConflict()
    {
        // Arrange
        var error = Error.Conflict("CONFLICT.ERROR", "Conflict occurred");
        var result = Result.Failure(error);

        // Act
        IResult httpResult = CustomResults.Problem(result);

        // Assert
        ProblemHttpResult problemResult = httpResult.Should().BeOfType<ProblemHttpResult>().Subject;
        problemResult.StatusCode.Should().Be(StatusCodes.Status409Conflict);
    }

    [Fact]
    public void Problem_Returns500_WhenErrorTypeIsFailure()
    {
        // Arrange
        var error = Error.Failure("FAILURE.ERROR", "General failure");
        var result = Result.Failure(error);

        // Act
        IResult httpResult = CustomResults.Problem(result);

        // Assert
        ProblemHttpResult problemResult = httpResult.Should().BeOfType<ProblemHttpResult>().Subject;
        problemResult.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }

    [Fact]
    public void Problem_Returns400_WhenErrorTypeIsProblem()
    {
        // Arrange
        var error = Error.Problem("PROBLEM.ERROR", "Problem occurred");
        var result = Result.Failure(error);

        // Act
        IResult httpResult = CustomResults.Problem(result);

        // Assert
        ProblemHttpResult problemResult = httpResult.Should().BeOfType<ProblemHttpResult>().Subject;
        problemResult.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public void Problem_IncludesErrorCode_InProblemDetails()
    {
        // Arrange
        var error = Error.Failure("CUSTOM.CODE", "Error description");
        var result = Result.Failure(error);

        // Act
        IResult httpResult = CustomResults.Problem(result);

        // Assert
        ProblemHttpResult problemResult = httpResult.Should().BeOfType<ProblemHttpResult>().Subject;
        problemResult.ProblemDetails.Extensions.Should().ContainKey("code");
        problemResult.ProblemDetails.Extensions["code"].Should().Be("CUSTOM.CODE");
    }

    [Fact]
    public void Problem_IncludesErrorDescription_InProblemDetails()
    {
        // Arrange
        var error = Error.Failure("CODE", "Detailed error description");
        var result = Result.Failure(error);

        // Act
        IResult httpResult = CustomResults.Problem(result);

        // Assert
        ProblemHttpResult problemResult = httpResult.Should().BeOfType<ProblemHttpResult>().Subject;
        problemResult.ProblemDetails.Detail.Should().Be("Detailed error description");
    }

    [Fact]
    public void Problem_SetsAppropriateTitle_ForNotFound()
    {
        // Arrange
        var error = Error.NotFound("CODE", "Description");
        var result = Result.Failure(error);

        // Act
        IResult httpResult = CustomResults.Problem(result);

        // Assert
        ProblemHttpResult problemResult = httpResult.Should().BeOfType<ProblemHttpResult>().Subject;
        problemResult.ProblemDetails.Title.Should().Be("Not Found");
    }

    [Fact]
    public void Problem_SetsAppropriateTitle_ForValidation()
    {
        // Arrange
        var error = Error.Validation("CODE", "Description");
        var result = Result.Failure(error);

        // Act
        IResult httpResult = CustomResults.Problem(result);

        // Assert
        ProblemHttpResult problemResult = httpResult.Should().BeOfType<ProblemHttpResult>().Subject;
        problemResult.ProblemDetails.Title.Should().Be("Bad Request");
    }

    [Fact]
    public void Problem_SetsAppropriateTitle_ForConflict()
    {
        // Arrange
        var error = Error.Conflict("CODE", "Description");
        var result = Result.Failure(error);

        // Act
        IResult httpResult = CustomResults.Problem(result);

        // Assert
        ProblemHttpResult problemResult = httpResult.Should().BeOfType<ProblemHttpResult>().Subject;
        problemResult.ProblemDetails.Title.Should().Be("Conflict");
    }

    [Fact]
    public void Problem_SetsAppropriateTitle_ForFailure()
    {
        // Arrange
        var error = Error.Failure("CODE", "Description");
        var result = Result.Failure(error);

        // Act
        IResult httpResult = CustomResults.Problem(result);

        // Assert
        ProblemHttpResult problemResult = httpResult.Should().BeOfType<ProblemHttpResult>().Subject;
        problemResult.ProblemDetails.Title.Should().Be("Internal Server Error");
    }

    [Fact]
    public void Problem_IncludesRFC7231TypeUri_ForNotFound()
    {
        // Arrange
        var error = Error.NotFound("CODE", "Description");
        var result = Result.Failure(error);

        // Act
        IResult httpResult = CustomResults.Problem(result);

        // Assert
        ProblemHttpResult problemResult = httpResult.Should().BeOfType<ProblemHttpResult>().Subject;
        problemResult.ProblemDetails.Type.Should().Be("https://tools.ietf.org/html/rfc7231#section-6.5.4");
    }

    [Fact]
    public void Problem_IncludesRFC7231TypeUri_ForValidation()
    {
        // Arrange
        var error = Error.Validation("CODE", "Description");
        var result = Result.Failure(error);

        // Act
        IResult httpResult = CustomResults.Problem(result);

        // Assert
        ProblemHttpResult problemResult = httpResult.Should().BeOfType<ProblemHttpResult>().Subject;
        problemResult.ProblemDetails.Type.Should().Be("https://tools.ietf.org/html/rfc7231#section-6.5.1");
    }

    [Fact]
    public void Problem_WorksWithGenericResult()
    {
        // Arrange
        var error = Error.NotFound("USER.NOTFOUND", "User not found");
        var result = Result.Failure<int>(error);

        // Act
        IResult httpResult = CustomResults.Problem(result);

        // Assert
        ProblemHttpResult problemResult = httpResult.Should().BeOfType<ProblemHttpResult>().Subject;
        problemResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);
        problemResult.ProblemDetails.Detail.Should().Be("User not found");
    }

    [Fact]
    public void Problem_WorksWithComplexTypes()
    {
        // Arrange
        var dto = new TestDto { Id = 1, Name = "Test" };
        var result = Result.Success(dto);

        // Act
        IResult httpResult = CustomResults.Problem(result);

        // Assert
        Ok<TestDto> okResult = httpResult.Should().BeOfType<Ok<TestDto>>().Subject;
        okResult.Value.Should().Be(dto);
    }

    [Fact]
    public void Problem_ReturnsConsistentResults_ForSameError()
    {
        // Arrange
        var error = Error.Validation("CODE", "Description");
        var result1 = Result.Failure(error);
        var result2 = Result.Failure(error);

        // Act
        IResult httpResult1 = CustomResults.Problem(result1);
        IResult httpResult2 = CustomResults.Problem(result2);

        // Assert
        ProblemHttpResult problem1 = httpResult1.Should().BeOfType<ProblemHttpResult>().Subject;
        ProblemHttpResult problem2 = httpResult2.Should().BeOfType<ProblemHttpResult>().Subject;
        problem1.StatusCode.Should().Be(problem2.StatusCode);
        problem1.ProblemDetails.Title.Should().Be(problem2.ProblemDetails.Title);
    }

    [Theory]
    [InlineData(ErrorType.NotFound, StatusCodes.Status404NotFound)]
    [InlineData(ErrorType.Validation, StatusCodes.Status400BadRequest)]
    [InlineData(ErrorType.Conflict, StatusCodes.Status409Conflict)]
    [InlineData(ErrorType.Failure, StatusCodes.Status500InternalServerError)]
    [InlineData(ErrorType.Problem, StatusCodes.Status400BadRequest)]
    public void Problem_MapsErrorTypeToCorrectStatusCode(ErrorType errorType, int expectedStatusCode)
    {
        // Arrange
        var error = new Error("CODE", "Description", errorType);
        var result = Result.Failure(error);

        // Act
        IResult httpResult = CustomResults.Problem(result);

        // Assert
        ProblemHttpResult problemResult = httpResult.Should().BeOfType<ProblemHttpResult>().Subject;
        problemResult.StatusCode.Should().Be(expectedStatusCode);
    }

    private class TestDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}

#pragma warning restore CS0618 // Type or member is obsolete