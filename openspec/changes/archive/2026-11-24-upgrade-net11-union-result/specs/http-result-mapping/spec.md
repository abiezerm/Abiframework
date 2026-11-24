# Spec Delta

## Purpose

Defines how operation outcomes are turned into ASP.NET Core HTTP responses: the status code for each error type, the problem details shape, and the helpers endpoints use to return results with as little code as possible.

## ADDED Requirements

### Requirement: Error type maps to status code
A failed result SHALL produce an RFC 7807 problem details response whose status, title and type follow the error type: NotFound gives 404, Validation gives 400, Conflict gives 409, Problem gives 400, and Failure or any other type gives 500.

#### Scenario: Not found
- **WHEN** a failed result with an error of type `NotFound` is converted to an HTTP response
- **THEN** the response status is 404 and the title is "Not Found"

#### Scenario: Unknown type
- **WHEN** a failed result has an error type with no specific mapping
- **THEN** the response status is 500 and the title is "Internal Server Error"

### Requirement: Problem details carry the error data
A problem details response SHALL put the error description in `detail` and the error code in an extension member named `code`.

#### Scenario: Code and description
- **WHEN** a failure with code `User.Exists` and description `Already exists` is converted
- **THEN** the response `detail` is `Already exists` and its `code` extension is `User.Exists`

### Requirement: Successful results map to 200
A successful value result SHALL produce a 200 response with the value as the body. A successful non-value result SHALL produce a 200 response with no body.

#### Scenario: Value success
- **WHEN** a successful `Result<UserDto>` is converted with the default mapping
- **THEN** the response status is 200 and the body is the serialized `UserDto`

#### Scenario: Non-value success
- **WHEN** a successful `Result` is converted with the default mapping
- **THEN** the response status is 200 with no body

### Requirement: One-call endpoint conversion
Endpoints SHALL be able to convert a result, or a task producing a result, into an HTTP response with a single `ToHttpResult()` call, without writing `await` or a temporary variable in the endpoint.

#### Scenario: Task of result in a lambda
- **WHEN** an endpoint lambda is written as `(cmd, handler, ct) => handler.Handle(cmd, ct).ToHttpResult()`
- **THEN** the endpoint returns 200 with the value on success and problem details on failure

### Requirement: Custom success response
`ToHttpResult` SHALL accept a success mapper that builds the success response from the value. The error mapping SHALL stay the same.

#### Scenario: Created response
- **WHEN** an endpoint calls `ToHttpResult(id => TypedResults.Created($"/users/{id}", id))` on a successful result
- **THEN** the response status is 201 with a `Location` header of `/users/{id}`

#### Scenario: Mapper not called on failure
- **WHEN** the same endpoint receives a failed result
- **THEN** the success mapper is not called and the response is the problem details for the error

### Requirement: Response metadata is discoverable
The response type of `ToHttpResult` SHALL describe both the success response and the problem response, so OpenAPI documents them without extra `Produces` annotations.

#### Scenario: OpenAPI lists both responses
- **WHEN** an endpoint returns `handler.Handle(query, ct).ToHttpResult()` for a `Result<UserDto>`
- **THEN** the generated OpenAPI operation lists a 200 response with `UserDto` and a problem details response

### Requirement: Existing conversion helpers keep working
`Match(onSuccess, onFailure)` and `CustomResults.Problem(result)` SHALL keep their 7.x signatures and behavior, so existing endpoint code such as `result.Match(Results.Ok, CustomResults.Problem)` compiles and responds the same.

#### Scenario: Match with CustomResults
- **WHEN** an endpoint returns `result.Match(Results.Ok, CustomResults.Problem)` for a failed result of type `Conflict`
- **THEN** the response status is 409

### Requirement: Map an error directly
`CustomResults.Problem` SHALL accept a `DomainError` directly, so a `switch` arm can map an error without wrapping it in a result.

#### Scenario: Switch arm
- **WHEN** an endpoint writes `result switch { Guid id => Results.Created($"/users/{id}", id), DomainError e => CustomResults.Problem(e) }` and the result is a Validation failure
- **THEN** the response status is 400
