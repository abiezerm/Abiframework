using AbiFramework.Entities;
using AbiFramework.Messaging;
using AbiFramework.Web;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace AbiFramework.Tests.Integration;

public class ReadmeExamplesTests
{
    [Fact]
    public async Task ResultPatternExamples_CompileAndMapBothCases()
    {
        Result<ReadmeUser> success = FindUser(1);
        Result<ReadmeUser> failure = FindUser(0);

        Describe(success).Should().Be("Ada");
        Describe(failure).Should().Be("The id must be positive");

        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        await using WebApplication app = builder.Build();
        MapEndpoint(app);
    }

    private static Result<ReadmeUser> FindUser(int id)
    {
        if (id <= 0)
        {
            var error = DomainError.Validation("User.Id", "The id must be positive");
            return error;
        }

        ReadmeUser user = LoadUser(id);
        return user;
    }

    private static string Describe(Result<ReadmeUser> result) => result switch
    {
        ReadmeUser user => user.Name,
        DomainError error => error.Description
    };

    private static ReadmeUser LoadUser(int id) => new(id, "Ada");

    private static void MapEndpoint(IEndpointRouteBuilder app) => app.MapPost("/api/users", async (
        CreateReadmeUserRequest request,
        ICommandHandler<CreateReadmeUserCommand, int> handler,
        CancellationToken ct) =>
    {
        var command = new CreateReadmeUserCommand(request.Name);
        return await handler.Handle(command, ct)
            .ToHttpResult(id => Results.Created($"/api/users/{id}", id));
    });

    private sealed record ReadmeUser(int Id, string Name);

    private sealed record CreateReadmeUserRequest(string Name);

    private sealed record CreateReadmeUserCommand(string Name) : ICommand<int>;
}
