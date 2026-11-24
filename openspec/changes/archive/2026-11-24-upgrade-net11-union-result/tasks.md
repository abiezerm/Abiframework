# Tasks

## 1. Toolchain and union spike

- [x] 1.1 Install the .NET 11 SDK, add `global.json` pinning it, and verify `dotnet --version` reports 11.0.x from the repo root
- [x] 1.2 In a throwaway console project outside the repo, declare `union Result<T>(T, DomainError) where T : notnull` with a body holding static factories, `IsSuccess`, `Error` and a `T Value` property. Verify it compiles and an exhaustive `switch` works. Record in design.md whether D1 (union keyword) or its fallback (custom `[Union]` struct) applies, and how D4 (`Value` clash) resolved.
- [x] 1.3 In the same spike, verify `return validationError;` (a `DomainError` subclass) converts to `Result<T>` and matches the `DomainError` arm, and record the outcome in design.md

## 2. Retarget to net11.0

- [x] 2.1 Change `TargetFramework` to `net11.0` in `Directory.Build.props` and `src/AbiFramework/AbiFramework.csproj`, and set `<Version>8.0.0</Version>`. Verify `dotnet build` restores for net11.0.
- [x] 2.2 Bump the Microsoft.* package versions in `Directory.Packages.props` to 11.0.x (including `Microsoft.AspNetCore.Mvc.Testing`) and `SonarAnalyzer.CSharp` to its newest release. Verify `dotnet build` and `dotnet test` pass on the current 7.x code before any `Result` change.

## 3. Union-based Result

- [x] 3.1 Add the `Unit` success marker (sealed class, single `Unit.Value` instance) in `Entities/`, and verify with a unit test that `Unit.Value` is a single shared instance
- [x] 3.2 Rewrite `Entities/Result.cs` as `union Result(Unit, DomainError)` and `union Result<T>(T, DomainError) where T : notnull`, with the compatibility members and factories (or the custom struct, per task 1.2). Verify that `ResultTests` cover every `result-pattern` scenario, including `default(Result<int>)` failing with `NullValue`, and that they pass.
- [x] 3.3 Add a test that a `switch` with both arms compiles without a discard. Verify the test project builds with no exhaustiveness warnings.
- [x] 3.4 Add `ValidationError.FromResults<T>(IEnumerable<Result<T>>)`, and verify `ValidationErrorTests` cover the ordered-collection scenario and pass
- [x] 3.5 Update `ValidationDecorator` to return the `DomainError` directly instead of `new Result<TResponse>(default, false, …)`, and `LoggingDecorator` to keep working with the new type. Verify `ValidationDecoratorTests` and `LoggingDecoratorTests` pass, including the short-circuit and pass-through scenarios.

## 4. HTTP mapping

- [x] 4.1 Move the status, title and type tables in `Web/CustomResults.cs` into one `ProblemHttpResult` builder, add a public `Problem(DomainError)`, and rewrite `Problem(Result)` / `Problem<T>(Result<T>)` as switches without `ThrowIfNull`. Verify `CustomResultsTests` pass, with the null-argument tests removed and a `Problem(DomainError)` test added.
- [x] 4.2 Rewrite `Extensions/ResultExtensions.Match` overloads as switches with unchanged signatures, and verify a test calling `result.Match(Results.Ok, CustomResults.Problem)` returns 409 for a Conflict failure
- [x] 4.3 Add `Web/ResultHttpExtensions.cs` with the `ToHttpResult` overloads (sync and `Task`, generic and non-generic, plus the success mapper) returning `Results<…, ProblemHttpResult>`. Verify unit tests cover the 200, 201 mapper, mapper-not-called-on-failure and non-value 200 scenarios.
- [x] 4.4 Extend `ResultIntegrationTests` with a minimal API host (`WebApplicationFactory`) mapping `handler.Handle(cmd, ct).ToHttpResult()`. Verify the HTTP status, the problem details `code` extension, and the OpenAPI document listing both the 200 and problem responses.

## 5. Documentation

- [x] 5.1 Update `README.md`: the .NET 11 requirement, union-based `Result` usage (`return value;`, `return error;`, exhaustive `switch`), `ToHttpResult` endpoint examples, a note that `Result` is not a DTO, and a "Migrating from 7.x" section listing the breaking changes from the proposal. Fix the existing `Result<User>.Failure(error)` example so it compiles. Verify the README snippets compile by copying them into a test.
- [x] 5.2 Update `tests/AbiFramework.Tests/README.md` and `tests/VERIFICATION_CHECKLIST.md` for the new and removed tests, and verify the listed counts match `dotnet test --list-tests`

## 6. Integration check

- [x] 6.1 Run `dotnet build -c Release` (warnings as errors) and `dotnet test` from the repo root, and verify both succeed and the 8.0.0 `.nupkg` / `.snupkg` are produced
- [x] 6.2 Point one consumer project at the local 8.0.0 package with its TFM on net11.0, and verify it builds with only the documented breaking changes to fix and that an endpoint rewritten with `ToHttpResult()` behaves the same as before

## Workflow follow-up

- Archive the change after the 8.0.0 PR is merged and the package is published.
