# Design

## Context

See proposal.md (Why) for motivation, and `specs/result-pattern` and `specs/http-result-mapping` for the required behavior.

Current state that shapes the approach:

- `Result` is a class with `IsSuccess` and `Error`, and `DomainError.None` marks success. `Result<T> : Result` adds `Value` (it throws on failure) and an implicit conversion from `T?`, where `null` becomes `DomainError.NullValue`.
- The library itself uses these members in `LoggingDecorator` (`IsSuccess`, `Error`), `ValidationDecorator` (`new Result<TResponse>(default, false, error)`, `Result.Failure`), `ValidationError.FromResults(IEnumerable<Result>)`, `ResultExtensions.Match` and `CustomResults.Problem`.
- `ValidationError` is a `record` that derives from `DomainError`.
- The library csproj suppresses CS0618 (obsolete) and does not treat warnings as errors. `Directory.Build.props` and probably the consumer templates set `TreatWarningsAsErrors=true`.
- The machine has SDKs up to 10.0.404. The .NET 11 SDK has to be installed first.

C# 15 union facts this design depends on (Microsoft Learn tutorial, updated 2026-10-23):

- `union X(A, B);` is shorthand for a struct holding one case in an `object? Value`. Value-type cases are boxed.
- The compiler adds a constructor and an implicit conversion for each case type, and checks `switch` expressions for exhaustiveness.
- A union declaration can have a body with properties and methods, and can declare generic constraints.
- `default(X)` has a null `Value`.
- **Not confirmed by the docs**: static members inside a union body, and two cases where one type derives from the other. Task group 1 checks both before anything else is written.

Task group 1 spike outcome (.NET SDK 11.0.100):

- The built-in `union Result<T>(T, DomainError)` syntax compiles with a compatibility body containing static factories, `IsSuccess` and `Error`, and exhaustive matching works. Declaring the required `T Value` member fails with CS0102 because the compiler also generates `object? Value`.
- Use the custom `[Union]` struct fallback for D1. An `IUnionMembers` provider exposes its required `object? Value` explicitly while `Result<T>` keeps the public compatibility `T Value`. Static `Create` members on the provider define the `T` and `DomainError` cases; static factories on `Result<T>` also compile.
- A `ValidationError` returned from a `Result<int>` method converts through the `DomainError` case and matches the `DomainError` arm in an exhaustive switch.

## Goals / Non-Goals

**Goals:**
- Use a C# 15 union type implemented as a custom `[Union]` struct so consumers get compiler-provided conversions and exhaustive `switch`.
- Keep every 7.x member used at call sites so consumer code compiles with no edits beyond the TFM bump. Exceptions are the breaking items listed in the proposal.
- Make endpoints a single expression with `ToHttpResult()`.
- Keep `AbiFramework.Entities` free of HTTP types.

**Non-Goals:**
- Removing or deprecating the compatibility members. That is a later change.
- Aggregating validation failures into a `ValidationError` inside `ValidationDecorator`. It keeps today's single joined `DomainError.Validation`.
- Generic endpoint mapping helpers (`MapCommand<,>` / `MapQuery<,>`).
- Avoiding boxing for value-type `T`.
- Changing `OperationResult<T>`, `PagedListResult<T>` or `SortResult`.

## Decisions

### D1. Custom `[Union]` struct with a compatibility surface (option C)

```csharp
[Union]
public struct Result<T> : Result<T>.IUnionMembers where T : notnull
{
    private readonly object? _value;

    public interface IUnionMembers
    {
        static Result<T> Create(T value) => ...;
        static Result<T> Create(DomainError value) => ...;
        object? Value { get; }
    }
    object? IUnionMembers.Value => _value;
    public bool IsSuccess => _value is T;
    public bool IsFailure => !IsSuccess;
    public T Value => ...;
    public DomainError Error => ...;
    public static Result<T> Success(T value) => value;
    public static Result<T> ValidationFailure(DomainError error) => error;
}
```

**Alternatives**:
- **Built-in `union` declaration (option B)**: concise, but the spike showed its generated `Value` conflicts with the required compatibility `T Value` member (CS0102).
- **Pure one-line built-in union (option A)**: simplest, but every consumer must rewrite `IsSuccess` / `Value` code at once.

The task 1.2 spike found the built-in declaration cannot provide the required compatibility surface, so the custom `[Union]` struct fallback was selected. Its `IUnionMembers` provider exposes the compiler-required `object? Value` while `Result<T>` retains the public typed compatibility members. The compiler still supplies conversions and exhaustive-switch checking.

### D2. Non-generic `Result` uses a `Unit` success case

`[Union] public struct Result : IUnion`, with `Unit` and `DomainError` cases, where `Unit` is a sealed class with one shared instance, `Unit.Value`.

- A class rather than a struct, so `Result.Success()` stores a shared reference and allocates nothing.
- Not called `Success`, which would clash with the `Result.Success()` factory.
- Not called `Ok`, which would clash with `Microsoft.AspNetCore.Http.HttpResults.Ok` in endpoint files.

Consumers can switch with `Unit => ..., DomainError e => ...`, or keep using `IsSuccess`.

### D3. `where T : notnull`, and an empty result counts as failure

The constraint removes the `null` arm that unconstrained generics would otherwise force into every `switch`. A null or `default` value can still get through a constructor or `default(...)`. Every compatibility member handles it as a failure with `DomainError.NullValue`, which matches today's behavior where converting from `null` gives `NullValue`.

**Alternative**: keep `T` unconstrained. Rejected, because the extra `null` arm in every `switch` cancels most of the simplification.

### D4. Member name clash on `Value`

The built-in `union` declaration exposes a generated `object? Value` property, which conflicts with the compatibility `T Value` member (CS0102). The selected `[Union]` struct uses an explicit `IUnionMembers.Value` implementation so it can retain its public typed `T Value`.

The public `Result<T>.Value` stays a `T` that throws on failure (spec: compatibility members).

### D5. `ToHttpResult` lives in `AbiFramework.Web` and returns typed results

`ResultHttpExtensions` in `src/AbiFramework/Web/`:

```csharp
Task<Results<Ok<T>, DomainProblemResult>> ToHttpResult<T>(this Task<Result<T>> task)
Task<Results<TOk, DomainProblemResult>>   ToHttpResult<T, TOk>(this Task<Result<T>> task, Func<T, TOk> onSuccess) where TOk : IResult
Task<Results<Ok, DomainProblemResult>>    ToHttpResult(this Task<Result> task)
// and the same overloads synchronously on Result / Result<T>
```

`ProblemHttpResult` executes problem responses but its ASP.NET Core 11 metadata provider adds no response metadata. A `Results<,>` union therefore infers its success response but not the problem response. `DomainProblemResult` wraps the shared `ProblemHttpResult` builder and implements `IEndpointMetadataProvider`, adding the 400, 404, 409, and 500 `ProblemDetails` responses with the `application/problem+json` content type. This keeps the response body and status mapping in the single `CustomResults` builder while making OpenAPI describe all possible domain-error statuses automatically.

**Alternatives**:
- **Endpoint filter**: OpenAPI would report `Result<T>` as the body.
- **`Result : IResult`**: would add an HTTP dependency to the Entities namespace.

### D6. `Match` and `CustomResults.Problem` keep their signatures

Their bodies become `switch` expressions. `ArgumentNullException.ThrowIfNull` is removed because a struct can't be null. The tests that checked for that exception are removed.

### D7. `ValidationError.FromResults` overload

Add `FromResults<T>(IEnumerable<Result<T>>)` next to the existing `FromResults(IEnumerable<Result>)`. Lists that mix result types can't be passed any more. That gap is listed as a consumer break in the proposal.

### D8. Build settings

- `TargetFramework` becomes `net11.0` in `Directory.Build.props` and `AbiFramework.csproj`.
- `LangVersion` stays `latest`, which means C# 15 on net11.0.
- Microsoft.* packages go to 11.0.x, including `Microsoft.AspNetCore.Mvc.Testing`, which is still on 9.0.0.
- `SonarAnalyzer.CSharp` goes to the newest version that supports C# 15 syntax.
- Leave `Microsoft.AspNetCore.Http.Features` 2.2.0 in place unless a build fails because of it.
- Add `global.json` pinning the 11.0 SDK so local builds and CI agree.

## Risks / Trade-offs

- **The built-in union-generated `Value` conflicts with the compatibility property.** → The spike confirmed the conflict, and D1 uses an explicit provider interface so the public `T Value` remains available.
- **A value-type `T` (`int`, `Guid`) is boxed, which adds an allocation per result.** → The 7.x class allocated anyway, so this is no regression. A custom struct can come later if profiling shows it matters.
- **Analyzers may not parse `union` (Sonar, IDE rules under `AnalysisMode=All`), and warnings fail the build.** → Bump the analyzers. Suppress a specific rule in the csproj only if it misfires on generated union members, and record the suppression in the PR.
- **`ValidationError` derives from `DomainError`.** It should match the `DomainError` case through implicit reference conversion. → Task 1.3 checks this, including that `return validationError;` resolves without ambiguity.
- **Consumers that serialize `Result` would get an unexpected shape**, because the union exposes `object? Value`. → Document in the README that `Result` is not a DTO. No serializer support is planned.
- **Consumers with `TreatWarningsAsErrors`** would break if compatibility members were marked `[Obsolete]`. → They are not marked in 8.0 (see Open Questions).

## Migration Plan

1. Install the .NET 11 SDK and pin it with `global.json`.
2. Ship 8.0.0 from a branch: TFM bump, `[Union]` `Result` structs, `ToHttpResult`, tests, and a README "Migrating from 7.x" section.
3. Consumers bump their TFM to `net11.0` and the package to 8.0.0, then fix only the breaks the proposal lists.
4. **Rollback**: consumers stay on 7.0.0 (net10.0). The two versions are independent packages, so nothing else needs undoing.

## Open Questions

- When should the compatibility members be marked `[Obsolete]` and removed? Decide after consumers have moved to `switch` / `ToHttpResult`. It doesn't affect this change.
