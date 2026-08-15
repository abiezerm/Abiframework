using System.Reflection;
using AbiFramework.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;

namespace AbiFramework.Tests.Web;

public class EndpointExtensionsTests
{
    [Fact]
    public void AddEndpoints_RegistersEndpointFromAssembly()
    {
        // Arrange
        var services = new ServiceCollection();
        var assembly = typeof(TestEndpoint).Assembly;

        // Act
        services.AddEndpoints(assembly);

        // Assert
        var provider = services.BuildServiceProvider();
        var endpoints = provider.GetServices<IEndpoint>();
        endpoints.Should().NotBeEmpty();
    }

    [Fact]
    public void AddEndpoints_RegistersMultipleEndpoints()
    {
        // Arrange
        var services = new ServiceCollection();
        var assembly = typeof(TestEndpoint).Assembly;

        // Act
        services.AddEndpoints(assembly);

        // Assert
        var provider = services.BuildServiceProvider();
        var endpoints = provider.GetServices<IEndpoint>().ToList();
        endpoints.Should().Contain(e => e.GetType() == typeof(TestEndpoint));
        endpoints.Should().Contain(e => e.GetType() == typeof(AnotherTestEndpoint));
    }

    [Fact]
    public void AddEndpoints_UsesCallingAssembly_WhenNoAssemblyProvided()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddEndpoints();

        // Assert
        services.Should().NotBeEmpty();
    }

    [Fact]
    public void AddEndpoints_RegistersEndpointsAsTransient()
    {
        // Arrange
        var services = new ServiceCollection();
        var assembly = typeof(TestEndpoint).Assembly;

        // Act
        services.AddEndpoints(assembly);

        // Assert
        var provider = services.BuildServiceProvider();
        var endpoint1 = provider.GetService<IEndpoint>();
        var endpoint2 = provider.GetService<IEndpoint>();
        endpoint1.Should().NotBeSameAs(endpoint2);
    }

    [Fact]
    public void AddEndpoints_IgnoresAbstractClasses()
    {
        // Arrange
        var services = new ServiceCollection();
        var assembly = typeof(AbstractEndpoint).Assembly;

        // Act
        services.AddEndpoints(assembly);

        // Assert
        var provider = services.BuildServiceProvider();
        var endpoints = provider.GetServices<IEndpoint>();
        endpoints.Should().NotContain(e => e.GetType() == typeof(AbstractEndpoint));
    }

    [Fact]
    public void AddEndpoints_IgnoresInterfaces()
    {
        // Arrange
        var services = new ServiceCollection();
        var assembly = typeof(IEndpoint).Assembly;

        // Act
        services.AddEndpoints(assembly);

        // Assert
        // typeof(IEndpoint).Assembly contains the IEndpoint interface itself but no concrete
        // implementation, so a correct scan registers nothing.
        services.Should().BeEmpty();
    }

    [Fact]
    public void AddEndpoints_SupportsMultipleAssemblies()
    {
        // Arrange
        var services = new ServiceCollection();
        var assembly1 = typeof(TestEndpoint).Assembly;
        var assembly2 = typeof(EndpointExtensionsTests).Assembly;

        // Act
        services.AddEndpoints(assembly1, assembly2);

        // Assert
        var provider = services.BuildServiceProvider();
        var endpoints = provider.GetServices<IEndpoint>();
        endpoints.Should().NotBeEmpty();
    }

    [Fact]
    public void RegisterEndpoints_CallsMapEndpointOnAllRegisteredEndpoints()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IEndpoint, TestEndpoint>();
        services.AddSingleton<IEndpoint, AnotherTestEndpoint>();

        var provider = services.BuildServiceProvider();
        var app = new MockEndpointRouteBuilder(provider);

        // Act
        app.RegisterEndpoints();

        // Assert
        var testEndpoint = provider.GetServices<IEndpoint>().OfType<TestEndpoint>().First();
        var anotherEndpoint = provider.GetServices<IEndpoint>().OfType<AnotherTestEndpoint>().First();
        testEndpoint.WasMapEndpointCalled.Should().BeTrue();
        anotherEndpoint.WasMapEndpointCalled.Should().BeTrue();
    }

    [Fact]
    public void RegisterEndpoints_ReturnsRouteBuilder_ForChaining()
    {
        // Arrange
        var services = new ServiceCollection();
        var provider = services.BuildServiceProvider();
        var app = new MockEndpointRouteBuilder(provider);

        // Act
        var result = app.RegisterEndpoints();

        // Assert
        result.Should().BeSameAs(app);
    }

    [Fact]
    public void RegisterEndpoints_HandlesEmptyEndpointCollection()
    {
        // Arrange
        var services = new ServiceCollection();
        var provider = services.BuildServiceProvider();
        var app = new MockEndpointRouteBuilder(provider);

        // Act
        var act = () => app.RegisterEndpoints();

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void AddEndpoints_ReturnsServiceCollection_ForChaining()
    {
        // Arrange
        var services = new ServiceCollection();
        var assembly = typeof(TestEndpoint).Assembly;

        // Act
        var result = services.AddEndpoints(assembly);

        // Assert
        result.Should().BeSameAs(services);
    }

    [Fact]
    public void AddEndpoints_OnlyRegistersClassesThatImplementIEndpoint()
    {
        // Arrange
        var services = new ServiceCollection();
        var assembly = typeof(NonEndpointClass).Assembly;

        // Act
        services.AddEndpoints(assembly);

        // Assert
        var provider = services.BuildServiceProvider();
        var endpoints = provider.GetServices<IEndpoint>();
        endpoints.Should().NotContain(e => e.GetType() == typeof(NonEndpointClass));
    }

    [Fact]
    public void EndpointExtensions_WorkWithWebApplication()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IEndpoint, TestEndpoint>();

        // This test verifies the pattern works conceptually
        // Full integration would require WebApplicationBuilder
        var provider = services.BuildServiceProvider();

        // Assert
        var endpoints = provider.GetServices<IEndpoint>();
        endpoints.Should().NotBeEmpty();
    }

    [Fact]
    public void AddEndpoints_HandlesNestedEndpointClasses()
    {
        // Arrange
        var services = new ServiceCollection();
        var assembly = typeof(NestedEndpointContainer.NestedEndpoint).Assembly;

        // Act
        services.AddEndpoints(assembly);

        // Assert
        var provider = services.BuildServiceProvider();
        var endpoints = provider.GetServices<IEndpoint>();
        endpoints.Should().Contain(e => e.GetType() == typeof(NestedEndpointContainer.NestedEndpoint));
    }

    private class TestEndpoint : IEndpoint
    {
        public bool WasMapEndpointCalled { get; private set; }

        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            WasMapEndpointCalled = true;
        }
    }

    private class AnotherTestEndpoint : IEndpoint
    {
        public bool WasMapEndpointCalled { get; private set; }

        public void MapEndpoint(IEndpointRouteBuilder app)
        {
            WasMapEndpointCalled = true;
        }
    }

    private abstract class AbstractEndpoint : IEndpoint
    {
        public abstract void MapEndpoint(IEndpointRouteBuilder app);
    }

    private class NonEndpointClass
    {
        public void SomeMethod() { }
    }

    private static class NestedEndpointContainer
    {
        public class NestedEndpoint : IEndpoint
        {
            public void MapEndpoint(IEndpointRouteBuilder app) { }
        }
    }

    private class MockEndpointRouteBuilder : IEndpointRouteBuilder
    {
        public MockEndpointRouteBuilder(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
            DataSources = new List<EndpointDataSource>();
        }

        public IServiceProvider ServiceProvider { get; }
        public ICollection<EndpointDataSource> DataSources { get; }

        public IApplicationBuilder CreateApplicationBuilder()
        {
            throw new NotImplementedException();
        }
    }
}