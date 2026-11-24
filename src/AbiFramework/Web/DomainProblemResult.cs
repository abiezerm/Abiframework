using System.Reflection;
using AbiFramework.Entities;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace AbiFramework.Web;

/// <summary>
/// Writes a domain problem response and describes its possible responses to endpoint metadata.
/// </summary>
public sealed class DomainProblemResult : IResult, IEndpointMetadataProvider, IStatusCodeHttpResult, IValueHttpResult<ProblemDetails>
{
    private static readonly int[] ProblemStatusCodes =
    [
        StatusCodes.Status400BadRequest,
        StatusCodes.Status404NotFound,
        StatusCodes.Status409Conflict,
        StatusCodes.Status500InternalServerError
    ];

    private readonly ProblemHttpResult _problem;

    public DomainProblemResult(DomainError error)
    {
        _problem = CustomResults.BuildProblemResult(error);
    }

    public int? StatusCode => _problem.StatusCode;

    public ProblemDetails Value => _problem.ProblemDetails;

    public Task ExecuteAsync(HttpContext httpContext) => _problem.ExecuteAsync(httpContext);

    public static void PopulateMetadata(MethodInfo method, EndpointBuilder builder)
    {
        foreach (int statusCode in ProblemStatusCodes)
        {
            builder.Metadata.Add(new ProducesResponseTypeMetadata(
                statusCode,
                typeof(ProblemDetails),
                ["application/problem+json"]));
        }
    }
}
