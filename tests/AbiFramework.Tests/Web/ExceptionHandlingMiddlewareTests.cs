using System.Text.Json;
using AbiFramework.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace AbiFramework.Tests.Web;

public class ExceptionHandlingMiddlewareTests
{
    private readonly Mock<ILogger<ExceptionHandlingMiddleware>> _logger;
    private readonly Mock<RequestDelegate> _next;

    public ExceptionHandlingMiddlewareTests()
    {
        _logger = new Mock<ILogger<ExceptionHandlingMiddleware>>();
        _next = new Mock<RequestDelegate>();
    }

    [Fact]
    public async Task InvokeAsync_CallsNextMiddleware_WhenNoExceptionOccurs()
    {
        // Arrange
        var middleware = new ExceptionHandlingMiddleware(_next.Object, _logger.Object);
        var context = new DefaultHttpContext();

        _next.Setup(n => n(It.IsAny<HttpContext>())).Returns(Task.CompletedTask);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _next.Verify(n => n(context), Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_CatchesException_AndReturns500()
    {
        // Arrange
        var middleware = new ExceptionHandlingMiddleware(_next.Object, _logger.Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        _next.Setup(n => n(It.IsAny<HttpContext>()))
            .ThrowsAsync(new InvalidOperationException("Test exception"));

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }

    [Fact]
    public async Task InvokeAsync_LogsError_WhenExceptionOccurs()
    {
        // Arrange
        var middleware = new ExceptionHandlingMiddleware(_next.Object, _logger.Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        var exception = new InvalidOperationException("Test exception");
        _next.Setup(n => n(It.IsAny<HttpContext>())).ThrowsAsync(exception);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        _logger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                exception,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task InvokeAsync_ReturnsJsonResponse_WhenExceptionOccurs()
    {
        // Arrange
        var middleware = new ExceptionHandlingMiddleware(_next.Object, _logger.Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        _next.Setup(n => n(It.IsAny<HttpContext>()))
            .ThrowsAsync(new InvalidOperationException("Test exception"));

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.ContentType.Should().Be("application/json");
    }

    [Fact]
    public async Task InvokeAsync_IncludesGenericMessage_WhenNotInDebugMode()
    {
        // Arrange
        var middleware = new ExceptionHandlingMiddleware(_next.Object, _logger.Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        _next.Setup(n => n(It.IsAny<HttpContext>()))
            .ThrowsAsync(new InvalidOperationException("Sensitive error message"));

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var response = JsonSerializer.Deserialize<ErrorResponse>(responseBody, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        response.Should().NotBeNull();
        response!.Detail.Should().Be("An error occurred while processing your request.");
    }

    [Fact]
    public async Task InvokeAsync_IncludesExceptionMessage_WhenInDebugMode()
    {
        // Arrange
        var middleware = new ExceptionHandlingMiddleware(_next.Object, _logger.Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.Request.Headers["X-Debug"] = "true";

        var exceptionMessage = "Detailed error message";
        _next.Setup(n => n(It.IsAny<HttpContext>()))
            .ThrowsAsync(new InvalidOperationException(exceptionMessage));

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var response = JsonSerializer.Deserialize<ErrorResponse>(responseBody, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        response.Should().NotBeNull();
        response!.Detail.Should().Be(exceptionMessage);
    }

    [Fact]
    public async Task InvokeAsync_IncludesStackTrace_WhenInDebugMode()
    {
        // Arrange
        var middleware = new ExceptionHandlingMiddleware(_next.Object, _logger.Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.Request.Headers["X-Debug"] = "true";

        _next.Setup(n => n(It.IsAny<HttpContext>()))
            .ThrowsAsync(new InvalidOperationException("Test exception"));

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var response = JsonSerializer.Deserialize<ErrorResponse>(responseBody, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        response.Should().NotBeNull();
        response!.StackTrace.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task InvokeAsync_DoesNotIncludeStackTrace_WhenNotInDebugMode()
    {
        // Arrange
        var middleware = new ExceptionHandlingMiddleware(_next.Object, _logger.Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        _next.Setup(n => n(It.IsAny<HttpContext>()))
            .ThrowsAsync(new InvalidOperationException("Test exception"));

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var response = JsonSerializer.Deserialize<ErrorResponse>(responseBody, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        response.Should().NotBeNull();
        response!.StackTrace.Should().BeNull();
    }

    [Fact]
    public async Task InvokeAsync_IncludesInnerException_WhenInDebugMode()
    {
        // Arrange
        var middleware = new ExceptionHandlingMiddleware(_next.Object, _logger.Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.Request.Headers["X-Debug"] = "true";

        var innerException = new ArgumentException("Inner exception message");
        var exception = new InvalidOperationException("Outer exception", innerException);
        _next.Setup(n => n(It.IsAny<HttpContext>())).ThrowsAsync(exception);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var response = JsonSerializer.Deserialize<ErrorResponse>(responseBody, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        response.Should().NotBeNull();
        response!.InnerException.Should().Be("Inner exception message");
    }

    [Fact]
    public async Task InvokeAsync_IncludesTraceId_InResponse()
    {
        // Arrange
        var middleware = new ExceptionHandlingMiddleware(_next.Object, _logger.Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.TraceIdentifier = "test-trace-id";

        _next.Setup(n => n(It.IsAny<HttpContext>()))
            .ThrowsAsync(new InvalidOperationException("Test exception"));

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var response = JsonSerializer.Deserialize<ErrorResponse>(responseBody, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        response.Should().NotBeNull();
        response!.TraceId.Should().Be("test-trace-id");
    }

    [Fact]
    public async Task InvokeAsync_IncludesRequestPath_InResponse()
    {
        // Arrange
        var middleware = new ExceptionHandlingMiddleware(_next.Object, _logger.Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        context.Request.Path = "/api/test";

        _next.Setup(n => n(It.IsAny<HttpContext>()))
            .ThrowsAsync(new InvalidOperationException("Test exception"));

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var response = JsonSerializer.Deserialize<ErrorResponse>(responseBody, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        response.Should().NotBeNull();
        response!.Instance.Should().Be("/api/test");
    }

    [Fact]
    public async Task InvokeAsync_SetsStatusTo500()
    {
        // Arrange
        var middleware = new ExceptionHandlingMiddleware(_next.Object, _logger.Object);
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        _next.Setup(n => n(It.IsAny<HttpContext>()))
            .ThrowsAsync(new InvalidOperationException("Test exception"));

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        context.Response.Body.Seek(0, SeekOrigin.Begin);
        var responseBody = await new StreamReader(context.Response.Body).ReadToEndAsync();
        var response = JsonSerializer.Deserialize<ErrorResponse>(responseBody, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        response.Should().NotBeNull();
        response!.Status.Should().Be(500);
    }

    [Fact]
    public void UseExceptionHandling_AddsMiddleware()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        var app = new ApplicationBuilder(services.BuildServiceProvider());

        // Act
        app.UseExceptionHandling();

        // Assert
        var middleware = app.Build();
        middleware.Should().NotBeNull();
    }

    [Fact]
    public void UseExceptionHandling_ReturnsApplicationBuilder_ForChaining()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging();
        var app = new ApplicationBuilder(services.BuildServiceProvider());

        // Act
        var result = app.UseExceptionHandling();

        // Assert
        result.Should().BeSameAs(app);
    }

    private class ErrorResponse
    {
        public int Status { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Detail { get; set; } = string.Empty;
        public string Instance { get; set; } = string.Empty;
        public string TraceId { get; set; } = string.Empty;
        public string? StackTrace { get; set; }
        public string? InnerException { get; set; }
    }
}