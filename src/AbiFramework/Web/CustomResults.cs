using AbiFramework.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace AbiFramework.Web;

/// <summary>
/// Maps operation results and domain errors to ASP.NET Core HTTP results.
/// </summary>
public static class CustomResults
{
    /// <summary>
    /// Converts a non-value result to an HTTP result.
    /// </summary>
    public static IResult Problem(Result result) =>
        result.IsSuccess
            ? TypedResults.Ok()
            : BuildProblem(result.Error);

    /// <summary>
    /// Converts a value result to an HTTP result.
    /// </summary>
    public static IResult Problem<T>(Result<T> result) where T : notnull =>
        result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : BuildProblem(result.Error);

    /// <summary>
    /// Creates an RFC 7807 problem response from a domain error.
    /// </summary>
    public static ProblemHttpResult Problem(DomainError error) => BuildProblem(error);

    internal static ProblemHttpResult BuildProblemResult(DomainError error) => BuildProblem(error);

    private static ProblemHttpResult BuildProblem(DomainError error)
    {
        var (statusCode, title, type) = error.Type switch
        {
            ErrorType.NotFound => (StatusCodes.Status404NotFound, "Not Found", "https://tools.ietf.org/html/rfc7231#section-6.5.4"),
            ErrorType.Validation => (StatusCodes.Status400BadRequest, "Bad Request", "https://tools.ietf.org/html/rfc7231#section-6.5.1"),
            ErrorType.Conflict => (StatusCodes.Status409Conflict, "Conflict", "https://tools.ietf.org/html/rfc7231#section-6.5.8"),
            ErrorType.Problem => (StatusCodes.Status400BadRequest, "Bad Request", "https://tools.ietf.org/html/rfc7231#section-6.5.1"),
            _ => (StatusCodes.Status500InternalServerError, "Internal Server Error", "https://tools.ietf.org/html/rfc7231#section-6.6.1")
        };

        return TypedResults.Problem(
            statusCode: statusCode,
            title: title,
            detail: error.Description,
            type: type,
            extensions: new Dictionary<string, object?>
            {
                { "code", error.Code }
            });
    }
}
