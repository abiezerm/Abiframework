# AbiFramework Tests

The xUnit test project targets .NET 11 and uses FluentAssertions. It covers the union-based `Result` API, behaviors, HTTP result mapping, and integration examples.

## Test coverage

`dotnet test --list-tests` currently reports **185 tests**:

| Area | Tests | Coverage |
| --- | ---: | --- |
| Behaviors | 21 | Logging and validation decorators, including short-circuit and pass-through behavior |
| Entities | 85 | Entities, domain errors, error types, union results, and validation error aggregation |
| Web | 61 | `CustomResults`, `ToHttpResult`, endpoint registration, and exception middleware |
| Integration | 18 | HTTP status and problem response body, OpenAPI response metadata, and compile-checked README examples |

The old `ErrorTests` file and constructor/null-result tests were removed. `ResultTests` now cover union conversions, compatibility members, default-value handling, and exhaustive switches. `ResultHttpExtensionsTests` cover 200 and 201 responses, failure mapping, and bodyless success. `ResultIntegrationTests` exercise a real minimal API host through `WebApplicationFactory`.

## Run tests

```bash
dotnet build tests/AbiFramework.Tests/AbiFramework.Tests.csproj
dotnet test tests/AbiFramework.Tests/AbiFramework.Tests.csproj
dotnet test tests/AbiFramework.Tests/AbiFramework.Tests.csproj --list-tests
dotnet test tests/AbiFramework.Tests/AbiFramework.Tests.csproj --collect:"XPlat Code Coverage"
```

Tests use Arrange-Act-Assert, descriptive xUnit names, and FluentAssertions. Test doubles use Moq where appropriate; integration tests use `Microsoft.AspNetCore.Mvc.Testing` and the ASP.NET Core test server.
