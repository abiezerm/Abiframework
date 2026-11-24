# E2E report: upgrade-net11-union-result (2026-11-24)

Servers: reservo API → http://localhost:5085 (net11.0, consuming local AbiFramework 8.0.0; Scalar at `/scalar`, OpenAPI at `/openapi/v1.json`); throwaway PostgreSQL (pgvector:pg17 podman container `e2e-union-pg`, 127.0.0.1:55432)
Account role: none (anonymous endpoints and OpenAPI/Scalar only)
Runner: Claude Code delegated runner (claude-sonnet-5-5, medium)

Endpoint under test: `POST /v1/customer-auth/phone/verify-code` (`VerifyPhoneCodeEndpoint`), the only reservo endpoint rewritten to `handler.Handle(command, ct).ToHttpResult()` at the time of the run (task 6.2 still open; Codex still editing). It is the only `ToHttpResult()`/`Match` call site in `src/`.

Method: `curl` against the running API (each request with a fresh lowercase `Idempotency-Key`), plus the OpenAPI JSON and the Scalar page in Orca's browser. One OTP row was seeded directly into the throwaway database (SHA-256 of the code, as `VerifyPhoneCodeCommandHandler.HashCode`) because SMS delivery is disabled (`request-code` returns 503 `RequestPhoneCode.DeliveryUnavailable`), so no real code can be issued.

| # | Spec / Scenario | Result | Evidence | Notes |
|---|---|---|---|---|
| 1 | http-result-mapping / Not found (404, "Not Found") | BLOCKED | | No reachable path in `VerifyPhoneCode` returns a NotFound error. OpenAPI lists 404 for the operation. Covered by `CustomResultsTests` and `ResultIntegrationTests` in the library. |
| 2 | http-result-mapping / Unknown type (500, "Internal Server Error") | BLOCKED | | Cannot trigger a `Failure`/unmapped error without changing code. OpenAPI lists 500. Library-covered. |
| 3 | http-result-mapping / Code and description | PASS | `curl`; response `{"type":"https://tools.ietf.org/html/rfc7231#section-6.5.1","title":"Bad Request","status":400,"detail":"This code is incorrect or has expired.","code":"VerifyPhoneCode.InvalidCode"}` | Handler `DomainError.Validation` mapped to 400, `detail` = description, `code` extension = error code. Also holds for the validator failure (`code: NotEmptyValidator`). `Content-Type: application/problem+json`. |
| 4 | http-result-mapping / Value success | PASS | `curl`: 200, `application/json`, body with `accessToken`, `accessTokenExpiresAtUtc`, `refreshToken`, `refreshTokenExpiresAtUtc` | Token values not recorded. |
| 5 | http-result-mapping / Non-value success | BLOCKED | | No rewritten endpoint returns a non-generic `Result`. Library-covered (task 4.3). |
| 6 | http-result-mapping / Task of result in a lambda | PASS | `VerifyPhoneCodeEndpoint.cs` line `return await handler.Handle(command, cancellationToken).ToHttpResult();`; 200 and 400 observed (#3, #4) | The endpoint uses an `async` lambda with `await`, which the spec says callers need not write, but the call site is still the single `ToHttpResult()` replacing the ternary. |
| 7 | http-result-mapping / Created response | BLOCKED | | No reservo endpoint uses the success-mapper overload. Library-covered (task 4.3). |
| 8 | http-result-mapping / Mapper not called on failure | BLOCKED | | Same as #7. |
| 9 | http-result-mapping / OpenAPI lists both responses | PASS | e2e/openapi-lists-both-responses.png | `/openapi/v1.json` for `verify-code`: `200` (`application/json`, `VerifyPhoneCodeResponse`) plus `400`, `404`, `409`, `500` as `application/problem+json` (`ProblemDetails`). Scalar renders the same list. |
| 10 | http-result-mapping / Match with CustomResults (409) | BLOCKED | | No `.Match(...)` call site in reservo. Library-covered (task 4.2). |
| 11 | http-result-mapping / Switch arm (400) | BLOCKED | | No `switch` arm call site in reservo. Library-covered (task 4.1). |

Passed: 4 / 11 browser-testable scenarios; 7 BLOCKED because no reservo endpoint exercises them (none FAIL).

Behavior parity check (extra, not a spec scenario): the same `Idempotency-Key` replayed twice returned a byte-identical 400 problem body, so `ToHttpResult()` output survives the idempotency middleware's buffering/replay the same way `CustomResults.Problem` output does (`request-code`, which still uses `CustomResults.Problem`, returns the same problem shape under the same middleware).

## Console / network findings

- Scalar page (`/scalar`): no console messages, no failed requests.
- API log: no exceptions during the runs. A pre-existing EF warning 20613 (`OpeningHoursRange` owned JSON collection) appears at startup; unrelated to this change.
- A first batch of my own requests returned empty 400 bodies. That was a bug in my test shell function (unquoted header word-splitting sent a malformed `Idempotency-Key` header). Re-run correctly, the responses are as in #3. Not a product issue.

## Not browser-testable

- All `result-pattern` scenarios (union Result and Result<T>, Unit, default(Result<T>) failing with `NullValue`, exhaustive `switch`, `ValidationError.FromResults`, decorators): pure library behavior with no HTTP or UI surface. Covered by the AbiFramework unit tests (`ResultTests`, `ValidationErrorTests`, `ValidationDecoratorTests`, `LoggingDecoratorTests`).
- http-result-mapping scenarios marked BLOCKED above (#1, #2, #5, #7, #8, #10, #11): reachable only through call sites reservo does not have. They are covered by `CustomResultsTests`, `ResultHttpExtensionsTests` and the minimal-API `ResultIntegrationTests` (task 4.4).

## Environment notes

- The reservo worktree was being edited by another agent during the run (task 6.2). The API was built from the working tree at the moment of the run (`dotnet build src/Platform.Api`: 0 warnings, 0 errors); later edits are not reflected.
- Left running: the `api` Orca terminal, the Scalar browser tab, and the podman container `e2e-union-pg` (port 55432).
