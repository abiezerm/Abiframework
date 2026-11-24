# Spec Delta

## Purpose

Defines how AbiFramework represents the outcome of an operation: either success, optionally carrying a value, or failure carrying a `DomainError`. Handlers and consumers rely on it for consistent, type-checked error handling.

## ADDED Requirements

### Requirement: Outcome holds exactly one case
A value result SHALL hold exactly one of a success value of type `T` or a `DomainError`. A non-value result SHALL hold exactly one of a success marker or a `DomainError`. Success with an error, or failure without one, SHALL NOT be constructible.

#### Scenario: Value result holds the value
- **WHEN** a `Result<int>` is created from the value `42`
- **THEN** it reports success and its value is `42`

#### Scenario: Value result holds the error
- **WHEN** a `Result<int>` is created from `DomainError.NotFound("User.NotFound", "User not found")`
- **THEN** it reports failure and its error is that same `DomainError`

#### Scenario: Non-value result success
- **WHEN** `Result.Success()` is called
- **THEN** the result reports success and holds no error

### Requirement: Implicit conversion from cases
The library SHALL convert a value of type `T` implicitly to a successful `Result<T>`, and a `DomainError` (including derived errors such as `ValidationError`) implicitly to a failed `Result<T>` or `Result`.

#### Scenario: Handler returns a value directly
- **WHEN** a method declared to return `Result<Guid>` executes `return id;` with a `Guid`
- **THEN** the caller receives a successful result holding that `Guid`

#### Scenario: Handler returns an error directly
- **WHEN** a method declared to return `Result<Guid>` executes `return DomainError.Conflict("User.Exists", "Already exists");`
- **THEN** the caller receives a failed result holding that error

#### Scenario: Derived error converts
- **WHEN** a method declared to return `Result` returns a `ValidationError`
- **THEN** the caller receives a failed result whose error is that `ValidationError`

### Requirement: Exhaustive pattern matching
Consumers SHALL be able to `switch` over a result with one arm for the success case and one arm for `DomainError`, with no discard arm. The compiler SHALL warn when either arm is missing.

#### Scenario: Complete switch compiles cleanly
- **WHEN** a consumer writes `result switch { Guid id => ..., DomainError e => ... }` on a `Result<Guid>`
- **THEN** the code compiles without an exhaustiveness warning

#### Scenario: Missing arm is reported
- **WHEN** a consumer writes a switch on a `Result<Guid>` that only handles `Guid`
- **THEN** the compiler reports that the `DomainError` case is not handled

### Requirement: Value type must be non-nullable
`Result<T>` SHALL require a non-nullable `T`, so a success value is never `null`.

#### Scenario: Nullable type argument is rejected
- **WHEN** a consumer declares `Result<User?>` with nullable reference types enabled
- **THEN** the compiler reports a nullability constraint warning

### Requirement: Uninitialized result is treated as failure
A result that holds no case, such as `default(Result<T>)`, SHALL report failure with `DomainError.NullValue`, and SHALL NOT report success.

#### Scenario: Default result
- **WHEN** a consumer reads `IsSuccess` and `Error` on `default(Result<int>)`
- **THEN** `IsSuccess` is `false` and `Error` is `DomainError.NullValue`

### Requirement: Compatibility members
Results SHALL keep `IsSuccess`, `IsFailure`, `Error` and `Value`, and the factories `Result.Success()`, `Result.Success<T>(value)`, `Result.Failure(error)`, `Result.Failure<T>(error)` and `Result<T>.ValidationFailure(error)`, with their 7.x behavior.

#### Scenario: Error on success returns None
- **WHEN** `Error` is read on a successful result
- **THEN** it returns `DomainError.None`

#### Scenario: Value on failure throws
- **WHEN** `Value` is read on a failed `Result<T>`
- **THEN** an `InvalidOperationException` is thrown with the message "The value of a failure result can't be accessed."

#### Scenario: Factories produce equivalent results
- **WHEN** `Result.Failure<int>(error)` is called
- **THEN** the result is equivalent to converting `error` implicitly to `Result<int>`

### Requirement: Collect validation errors from value results
`ValidationError.FromResults` SHALL accept a sequence of `Result` or a sequence of `Result<T>`, and SHALL collect the errors of the failed items in order.

#### Scenario: Mixed success and failure
- **WHEN** `ValidationError.FromResults` receives three `Result<int>` items, where the first and third failed
- **THEN** the returned `ValidationError` contains those two errors in order

### Requirement: Pipeline decorators keep their outcomes
The validation decorator SHALL return a failed result holding a `DomainError` of type `Validation` when validation fails. The logging decorator SHALL log success or failure from the result's outcome. Neither SHALL change a result returned by the inner handler.

#### Scenario: Validation failure short-circuits
- **WHEN** a command with a value response fails validation
- **THEN** the inner handler is not called, and the result is a failure with an error of type `Validation`

#### Scenario: Inner result passes through
- **WHEN** a command passes validation and the inner handler returns a successful `Result<Guid>`
- **THEN** the decorator returns that same success value
