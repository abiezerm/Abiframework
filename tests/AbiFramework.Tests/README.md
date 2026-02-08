# AbiFramework Tests

Comprehensive unit tests for the AbiFramework library covering all v5.0.0 features.

## Test Coverage

### Behaviors
- **LoggingDecoratorTests**: Tests for query and command handler logging decorator
  - Success and failure logging
  - Execution time tracking
  - Exception handling

- **ValidationDecoratorTests**: Tests for command handler validation decorator
  - FluentValidation integration
  - Multiple validator support
  - Validation failure handling

### Entities
- **AEntityTests**: Tests for base entity with domain events
  - Domain event management
  - Event raising and clearing
  - Multiple primary key type support

- **DomainErrorTests**: Tests for modern error handling
  - Factory methods (Failure, NotFound, Validation, Problem, Conflict)
  - Record equality
  - Static instances

- **ErrorTests**: Tests for backward-compatible error class
  - Compatibility with DomainError
  - All error type factory methods

- **ErrorTypeTests**: Tests for error type enumeration
  - Enum values and conversions
  - Switch expression support

- **ResultTests**: Tests for Result pattern
  - Success and failure results
  - Generic Result<T>
  - Implicit conversions
  - Value access safety

- **ValidationErrorTests**: Tests for aggregated validation errors
  - Error collection
  - FromResults factory method
  - Integration with Result pattern

### Web
- **CustomResultsTests**: Tests for ASP.NET Core IResult conversion
  - HTTP status code mapping
  - Problem details generation
  - RFC 7231 compliance

- **EndpointExtensionsTests**: Tests for endpoint registration
  - Assembly scanning
  - Multiple endpoint registration
  - Transient lifetime verification

- **ExceptionHandlingMiddlewareTests**: Tests for global exception handling
  - Exception catching and logging
  - Debug mode support
  - JSON response formatting

### Integration
- **ResultIntegrationTests**: End-to-end integration tests
  - Result pattern with Web API
  - Error chaining
  - LINQ support
  - Guard clause patterns

## Running Tests

### Build the test project
```bash
dotnet build tests/AbiFramework.Tests/AbiFramework.Tests.csproj
```

### Run all tests
```bash
dotnet test tests/AbiFramework.Tests/AbiFramework.Tests.csproj
```

### Run tests with coverage
```bash
dotnet test tests/AbiFramework.Tests/AbiFramework.Tests.csproj --collect:"XPlat Code Coverage"
```

### Run tests in a specific namespace
```bash
dotnet test tests/AbiFramework.Tests/AbiFramework.Tests.csproj --filter "FullyQualifiedName~AbiFramework.Tests.Behaviors"
```

### Run specific test
```bash
dotnet test tests/AbiFramework.Tests/AbiFramework.Tests.csproj --filter "FullyQualifiedName~LoggingDecoratorTests.QueryHandler_LogsInformation_WhenHandlingSuccessfully"
```

## Test Framework

- **xUnit**: Test framework
- **FluentAssertions**: Assertion library for readable tests
- **Moq**: Mocking framework for dependencies
- **Microsoft.AspNetCore.Mvc.Testing**: ASP.NET Core testing utilities

## Test Organization

Tests are organized by namespace matching the source code structure:
- `AbiFramework.Tests.Behaviors` - Decorator tests
- `AbiFramework.Tests.Entities` - Entity and error handling tests
- `AbiFramework.Tests.Web` - Web infrastructure tests
- `AbiFramework.Tests.Integration` - Integration tests

## Code Coverage Goals

- Minimum 80% code coverage
- 100% coverage for critical paths (Result pattern, error handling)
- All public APIs tested
- Edge cases and boundary conditions covered

## Contributing

When adding new tests:
1. Follow existing naming conventions: `MethodName_Scenario_ExpectedResult`
2. Use Arrange-Act-Assert pattern
3. Keep tests focused on a single behavior
4. Use FluentAssertions for readable assertions
5. Mock external dependencies with Moq