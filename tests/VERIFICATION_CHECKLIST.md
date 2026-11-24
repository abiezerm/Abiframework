# Test Suite Verification Checklist

## Project setup

- [x] Test project targets `net11.0` and is included in `AbiFramework.slnx`.
- [x] xUnit, FluentAssertions, Moq, and ASP.NET Core testing dependencies are configured.
- [x] Analyzer settings remain enabled with warnings treated as errors.

## Result and domain behavior

- [x] `Unit.Value` is a shared singleton.
- [x] `Result` and `Result<T>` success/error conversions and compatibility members are covered.
- [x] `default(Result<T>)` is treated as `DomainError.NullValue`.
- [x] Exhaustive success/error switches compile without a discard arm.
- [x] Validation errors aggregate ordered non-generic and generic results.
- [x] Validation and logging decorators cover short-circuit and pass-through paths.

## HTTP and integration behavior

- [x] `CustomResults` covers all domain error status mappings and problem details.
- [x] `ToHttpResult` covers 200, mapped 201, mapper-not-called-on-failure, and bodyless success.
- [x] `WebApplicationFactory` verifies HTTP status and the problem `code` extension.
- [x] OpenAPI output includes success and problem responses without endpoint `.ProducesProblem()` calls.
- [x] README result, switch, and endpoint examples are compiled by `ReadmeExamplesTests`.

## Current test inventory

Verified with `dotnet test tests/AbiFramework.Tests/AbiFramework.Tests.csproj --list-tests` on .NET 11:

| Area | Listed tests |
| --- | ---: |
| Behaviors | 21 |
| Entities | 85 |
| Web | 61 |
| Integration | 18 |
| **Total** | **185** |

The listing contains six command/header lines and 185 fully qualified test names. The current files are:

- Behaviors: `LoggingDecoratorTests`, `ValidationDecoratorTests`
- Entities: `AEntityTests`, `DomainErrorTests`, `ErrorTypeTests`, `ResultTests`, `ValidationErrorTests`
- Web: `CustomResultsTests`, `EndpointExtensionsTests`, `ExceptionHandlingMiddlewareTests`, `ResultHttpExtensionsTests`
- Integration: `ReadmeExamplesTests`, `ResultIntegrationTests`

## Verification commands

```bash
dotnet build -c Release
dotnet test
dotnet test tests/AbiFramework.Tests/AbiFramework.Tests.csproj --list-tests
```
