using AbiFramework.Entities;
using Microsoft.AspNetCore.Http;

namespace AbiFramework.Web;

#pragma warning disable CS0618 // Type or member is obsolete

/// <summary>
/// Provides helper methods for converting <see cref="Result"/> and <see cref="Result{T}"/> to ASP.NET Core <see cref="IResult"/>.
/// Maps error types to appropriate HTTP status codes.
/// </summary>
public static class CustomResults
{
    /// <summary>
    /// Converts a <see cref="Result"/> to an <see cref="IResult"/> with appropriate HTTP status code.
    /// </summary>
    /// <param name="result">The result to convert.</param>
    /// <returns>An <see cref="IResult"/> representing the result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when result is null.</exception>
    public static IResult Problem(Result result)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (result.IsSuccess)
        {
            return Results.Ok();
        }

        return CreateProblemResult(result.Error);
    }

    /// <summary>
    /// Converts a <see cref="Result{T}"/> to an <see cref="IResult"/> with appropriate HTTP status code.
    /// </summary>
    /// <typeparam name="T">The type of the result value.</typeparam>
    /// <param name="result">The result to convert.</param>
    /// <returns>An <see cref="IResult"/> representing the result.</returns>
    /// <exception cref="ArgumentNullException">Thrown when result is null.</exception>
    public static IResult Problem<T>(Result<T> result)
    {
        ArgumentNullException.ThrowIfNull(result);

        if (result.IsSuccess)
        {
            return Results.Ok(result.Value);
        }

        return CreateProblemResult(result.Error);
    }

    /// <summary>
    /// Creates a problem result from an error with the appropriate HTTP status code.
    /// </summary>
    /// <param name="error">The error to convert.</param>
    /// <returns>An <see cref="IResult"/> with the appropriate HTTP status code.</returns>
    private static IResult CreateProblemResult(Error error)
    {
        var statusCode = error.Type switch
        {
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Failure => StatusCodes.Status500InternalServerError,
            ErrorType.Problem => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };

        return Results.Problem(
            statusCode: statusCode,
            title: GetTitleForErrorType(error.Type),
            detail: error.Description,
            type: GetTypeForErrorType(error.Type),
            extensions: new Dictionary<string, object?>
            {
                { "code", error.Code }
            });
    }

    /// <summary>
    /// Gets the title for an error type.
    /// </summary>
    /// <param name="errorType">The error type.</param>
    /// <returns>The title for the error type.</returns>
    private static string GetTitleForErrorType(ErrorType errorType) =>
        errorType switch
        {
            ErrorType.NotFound => "Not Found",
            ErrorType.Validation => "Bad Request",
            ErrorType.Conflict => "Conflict",
            ErrorType.Failure => "Internal Server Error",
            ErrorType.Problem => "Bad Request",
            _ => "Internal Server Error"
        };

    /// <summary>
    /// Gets the type URI for an error type.
    /// </summary>
    /// <param name="errorType">The error type.</param>
    /// <returns>The type URI for the error type.</returns>
    private static string GetTypeForErrorType(ErrorType errorType) =>
        errorType switch
        {
            ErrorType.NotFound => "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            ErrorType.Validation => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            ErrorType.Conflict => "https://tools.ietf.org/html/rfc7231#section-6.5.8",
            ErrorType.Failure => "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            ErrorType.Problem => "https://tools.ietf.org/html/rfc7231#section-6.5.1",
            _ => "https://tools.ietf.org/html/rfc7231#section-6.6.1"
        };
}
