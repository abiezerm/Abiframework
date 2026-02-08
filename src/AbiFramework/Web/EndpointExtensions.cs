using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace AbiFramework.Web;

/// <summary>
/// Extension methods for registering and mapping endpoints.
/// </summary>
public static class EndpointExtensions
{
    /// <summary>
    /// Adds all implementations of <see cref="IEndpoint"/> from the specified assemblies to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add endpoints to.</param>
    /// <param name="assemblies">The assemblies to scan for endpoint implementations. If not provided, scans the calling assembly.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddEndpoints(
        this IServiceCollection services,
        params Assembly[] assemblies)
    {
        var assemblyList = assemblies.Length > 0 ? assemblies : new[] { Assembly.GetCallingAssembly() };

        var endpointTypes = assemblyList
            .SelectMany(a => a.GetTypes())
            .Where(t => t is { IsClass: true, IsAbstract: false } &&
                        t.GetInterfaces().Contains(typeof(IEndpoint)));

        foreach (var type in endpointTypes)
        {
            services.AddTransient(typeof(IEndpoint), type);
        }

        return services;
    }

    /// <summary>
    /// Maps all registered <see cref="IEndpoint"/> implementations to the endpoint route builder.
    /// </summary>
    /// <param name="app">The endpoint route builder to map endpoints to.</param>
    /// <returns>The endpoint route builder for method chaining.</returns>
    public static IEndpointRouteBuilder RegisterEndpoints(this IEndpointRouteBuilder app)
    {
        var endpoints = app.ServiceProvider.GetServices<IEndpoint>();

        foreach (var endpoint in endpoints)
        {
            endpoint.MapEndpoint(app);
        }

        return app;
    }
}
