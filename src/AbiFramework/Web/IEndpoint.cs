using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace AbiFramework.Web;

/// <summary>
/// Defines a contract for mapping endpoints to the application's endpoint route builder.
/// Implement this interface to define API endpoints in a modular way.
/// </summary>
public interface IEndpoint
{
    /// <summary>
    /// Maps the endpoint to the specified route builder.
    /// </summary>
    /// <param name="app">The endpoint route builder to map the endpoint to.</param>
    void MapEndpoint(IEndpointRouteBuilder app);
}
