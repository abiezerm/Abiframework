using AbiFramework.Entities;
using AbiFramework.Web;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace AbiFramework.Tests.Web;

public class ResultHttpExtensionsTests
{
    [Fact]
    public void ToHttpResult_WithValue_ReturnsOkWithValue()
    {
        Results<Ok<int>, DomainProblemResult> result = Result.Success(42).ToHttpResult();

        Ok<int> okResult = result.Result.Should().BeOfType<Ok<int>>().Subject;
        okResult.Value.Should().Be(42);
    }

    [Fact]
    public void ToHttpResult_WithSuccessMapper_ReturnsCreated()
    {
        Results<Created<int>, DomainProblemResult> result = Result.Success(42)
            .ToHttpResult(value => TypedResults.Created($"/values/{value}", value));

        Created<int> createdResult = result.Result.Should().BeOfType<Created<int>>().Subject;
        createdResult.StatusCode.Should().Be(StatusCodes.Status201Created);
        createdResult.Location.Should().Be("/values/42");
        createdResult.Value.Should().Be(42);
    }

    [Fact]
    public void ToHttpResult_WithFailure_DoesNotCallSuccessMapper()
    {
        var error = DomainError.Conflict("VALUE.CONFLICT", "Value conflicts");
        bool mapperCalled = false;

        Results<Created<int>, DomainProblemResult> result = Result.Failure<int>(error)
            .ToHttpResult(value =>
            {
                mapperCalled = true;
                return TypedResults.Created($"/values/{value}", value);
            });

        DomainProblemResult problemResult = result.Result.Should().BeOfType<DomainProblemResult>().Subject;
        mapperCalled.Should().BeFalse();
        problemResult.StatusCode.Should().Be(StatusCodes.Status409Conflict);
        problemResult.Value.Extensions["code"].Should().Be("VALUE.CONFLICT");
    }

    [Fact]
    public void ToHttpResult_WithoutValue_ReturnsBodylessOk()
    {
        Results<Ok, DomainProblemResult> result = Result.Success().ToHttpResult();

        result.Result.Should().BeOfType<Ok>();
    }

    [Fact]
    public async Task ToHttpResult_TaskOfValueResult_ReturnsOkWithValue()
    {
        Task<Result<int>> task = Task.FromResult(Result.Success(42));

        Results<Ok<int>, DomainProblemResult> result = await task.ToHttpResult();

        Ok<int> okResult = result.Result.Should().BeOfType<Ok<int>>().Subject;
        okResult.Value.Should().Be(42);
    }

    [Fact]
    public async Task ToHttpResult_TaskOfValueResultWithMapper_ReturnsCreated()
    {
        Task<Result<int>> task = Task.FromResult(Result.Success(42));

        Results<Created<int>, DomainProblemResult> result = await task.ToHttpResult(
            value => TypedResults.Created($"/values/{value}", value));

        Created<int> createdResult = result.Result.Should().BeOfType<Created<int>>().Subject;
        createdResult.StatusCode.Should().Be(StatusCodes.Status201Created);
    }

    [Fact]
    public async Task ToHttpResult_TaskOfResult_ReturnsBodylessOk()
    {
        Task<Result> task = Task.FromResult(Result.Success());

        Results<Ok, DomainProblemResult> result = await task.ToHttpResult();

        result.Result.Should().BeOfType<Ok>();
    }
}
