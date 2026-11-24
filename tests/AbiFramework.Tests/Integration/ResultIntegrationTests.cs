using AbiFramework.Entities;
using AbiFramework.Messaging;
using AbiFramework.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net;
using System.Text.Json;

namespace AbiFramework.Tests.Integration;

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
        var error = DomainError.NotFound("USER.NOTFOUND", "User not found");
        var result = Result.Failure<int>(error);

        // Act
        IResult httpResult = CustomResults.Problem(result);

        // Assert
        ProblemHttpResult problemResult = httpResult.Should().BeOfType<ProblemHttpResult>().Subject;
        problemResult.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public void Result_SupportsMatchPattern()
    {
        // Arrange
        var successResult = Result.Success(42);
        var failureResult = Result.Failure<int>(DomainError.Failure("CODE", "Description"));

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
        DomainError[] errors = new[]
        {
            DomainError.Validation("FIELD1.REQUIRED", "Field 1 is required"),
            DomainError.Validation("FIELD2.INVALID", "Field 2 is invalid")
        };
        var validationError = new ValidationError(errors);
        var result = Result.Failure(validationError);

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
        var error = new DomainError("CODE", "Description", errorType);
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
        nullResult.Error.Should().Be(DomainError.NullValue);
        valueResult.IsSuccess.Should().BeTrue();
        valueResult.Value.Should().Be("test");
    }

    [Fact]
    public void ErrorChaining_PreservesInformation()
    {
        // Arrange
        Result[] results = new[]
        {
            Result.Failure(DomainError.Validation("FIELD1.REQUIRED", "Field 1 required")),
            Result.Failure(DomainError.Validation("FIELD2.REQUIRED", "Field 2 required"))
        };

        // Act
        var validationError = ValidationError.FromResults(results);
        var finalResult = Result.Failure(validationError);

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
            Result.Failure<int>(DomainError.Failure("CODE", "Description")),
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

    [Fact]
    public async Task ToHttpResult_MapsSuccessAndProblemResponsesOverHttp()
    {
        using ResultApiFactory factory = new();
        using HttpClient client = factory.CreateClient();

        HttpResponseMessage success = await client.GetAsync("/results/42");
        string successBody = await success.Content.ReadAsStringAsync();
        HttpResponseMessage failure = await client.GetAsync("/results/0");
        using var problem = JsonDocument.Parse(await failure.Content.ReadAsStringAsync());

        success.StatusCode.Should().Be(HttpStatusCode.OK);
        successBody.Should().Be("42");
        failure.StatusCode.Should().Be(HttpStatusCode.NotFound);
        problem.RootElement.GetProperty("code").GetString().Should().Be("RESULT.NOTFOUND");
    }

    [Fact]
    public async Task ToHttpResult_OpenApiListsSuccessAndProblemResponses()
    {
        using ResultApiFactory factory = new();
        using HttpClient client = factory.CreateClient();
        using var document = JsonDocument.Parse(await client.GetStringAsync("/openapi/v1.json"));

        JsonElement responses = document.RootElement
            .GetProperty("paths")
            .GetProperty("/results/{id}")
            .GetProperty("get")
            .GetProperty("responses");

        responses.TryGetProperty("200", out _).Should().BeTrue();
        responses.TryGetProperty("500", out _).Should().BeTrue();
    }

    private static Result<string> GetNullableString(string? value)
    {
        return value!;
    }

    private static Result<User> GetUserById(int id)
    {
        if (id == 999)
        {
            return Result.Failure<User>(DomainError.NotFound("USER.NOTFOUND", "User not found"));
        }

        return Result.Success(new User { Id = id, Name = "Test User" });
    }

    public sealed record ResultCommand(int Id) : ICommand<int>;

    public sealed class ResultCommandHandler : ICommandHandler<ResultCommand, int>
    {
        public Task<Result<int>> Handle(ResultCommand command, CancellationToken cancellationToken) =>
            Task.FromResult<Result<int>>(command.Id == 0
                ? DomainError.NotFound("RESULT.NOTFOUND", "Result not found")
                : command.Id);
    }

    public sealed class ResultApiFactory : WebApplicationFactory<ResultIntegrationTests>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder) =>
            builder.UseContentRoot(AppContext.BaseDirectory);

        protected override IHostBuilder CreateHostBuilder() => new HostBuilder()
            .ConfigureWebHost(webHostBuilder => webHostBuilder
                .UseEnvironment("Development")
                .UseContentRoot(AppContext.BaseDirectory)
                .UseTestServer()
                .ConfigureServices(services =>
                {
                    services.AddRouting();
                    services.AddSingleton<ICommandHandler<ResultCommand, int>, ResultCommandHandler>();
                    services.AddOpenApi();
                })
                .Configure(app =>
                {
                    app.UseRouting();
                    app.UseEndpoints(endpoints =>
                    {
                        endpoints.MapGet("/results/{id:int}", (int id,
                            ICommandHandler<ResultCommand, int> handler,
                            CancellationToken cancellationToken) =>
                            handler.Handle(new ResultCommand(id), cancellationToken).ToHttpResult());
                        endpoints.MapOpenApi();
                    });
                }));
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
