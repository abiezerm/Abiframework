# Proposal

## Why

`Result` and `Result<T>` are hand-built classes. They track success with a flag and use `DomainError.None` as a sentinel, so states like "success with an error" have to be blocked at runtime by a constructor check. C# 15 on .NET 11 adds `union` types: a value is exactly one of a closed set of types, and conversions and exhaustive `switch` checks come from the compiler. All projects that use AbiFramework are moving to .NET 11 together, so the library can switch now without multi-targeting. That makes handler and endpoint code in those projects shorter, with fewer ways to get it wrong.

## What Changes

- Target `net11.0` (C# 15) in the library and test project, and move the Microsoft.* package references to their 11.x versions.
- **BREAKING**: `Result<T>` becomes a union of `T` and `DomainError`. It changes from a class to a struct and no longer inherits from `Result`.
- **BREAKING**: The non-generic `Result` becomes a union of a success marker and `DomainError`.
- **BREAKING**: `Result<T>` requires `T : notnull`. A `Result` can no longer be `null`, so `CustomResults.Problem` no longer throws `ArgumentNullException`.
- **BREAKING**: `ICommand<TResponse>`, `IQuery<TResponse>` and their handlers now require `TResponse : notnull`.
- **BREAKING**: The public `(value, isSuccess, error)` constructors are removed. Use the implicit conversions or the static factories instead.
- **Kept for compatibility**: `IsSuccess`, `IsFailure`, `Error`, `Value`, `Result.Success()`, `Result.Success<T>(value)`, `Result.Failure(error)`, `Result.Failure<T>(error)`, `Result<T>.ValidationFailure(error)`, `Match(...)` and `CustomResults.Problem(...)`. Existing call sites still compile.
- **New**: The compiler converts `T` and `DomainError` to a result implicitly, so handlers can `return value;` or `return error;`.
- **New**: Consumers can use an exhaustive `switch` over `T` / `DomainError`, and the compiler warns when a case is missing.
- **New**: `ToHttpResult()` extensions on `Result`, `Result<T>`, `Task<Result>` and `Task<Result<T>>`. They return ASP.NET Core typed results (`Results<Ok<T>, DomainProblemResult>`) so OpenAPI metadata is inferred; `DomainProblemResult` supplies the problem response metadata. An overload takes a success mapper for responses such as `201 Created`.
- **New**: `CustomResults.Problem(DomainError)` is public, so a `switch` arm can map an error directly.
- `ValidationError.FromResults` gets an overload for `IEnumerable<Result<T>>`, because `Result<T>` is no longer a `Result`.
- Package version goes to **8.0.0**.

## Capabilities

### New Capabilities

- `result-pattern`: How the library represents the outcome of an operation: success with an optional value, or failure with a `DomainError`. Covers construction, conversions, exhaustive matching and the compatibility members.
- `http-result-mapping`: How outcomes become ASP.NET Core HTTP responses: status code per error type, problem details shape, `Match`, `CustomResults.Problem` and `ToHttpResult`.

### Modified Capabilities

None. The project has no specs yet.

## Impact

- **Code**: `src/AbiFramework/Entities/Result.cs` (rewrite), `Entities/ValidationError.cs`, `Extensions/ResultExtensions.cs`, `Web/CustomResults.cs`, a new `Web` extension file for `ToHttpResult`, `Behaviors/ValidationDecorator.cs`, `Behaviors/LoggingDecorator.cs`.
- **Not affected**: `Web/EndpointExtensions.cs` and `Web/IEndpoint.cs` (endpoint discovery and mapping), and the `ICommandHandler` / `IQueryHandler` signatures, which still return `Task<Result>` / `Task<Result<T>>`.
- **Build**: `Directory.Build.props`, `AbiFramework.csproj`, `Directory.Packages.props`. This needs the .NET 11 SDK, which is not installed on the development machine yet. Analyzer packages must understand the C# 15 syntax.
- **Tests**: `ResultTests`, `ResultIntegrationTests`, `CustomResultsTests`, `ValidationErrorTests` and the decorator tests need updating for struct semantics and the new API.
- **Consumers**: These must move to `net11.0`. Code passing `Result<T>` where a `Result` is expected, using `Result<T?>` with a nullable `T`, calling the removed constructors, or null-checking a `Result` must change. `README.md` must document the new usage.
