namespace AbiFramework.Entities;

public sealed record ValidationError(DomainError[] Errors) : DomainError("Validation.General",
    "One or more validation errors occurred",
    ErrorType.Validation)
{
    public static ValidationError FromResults(IEnumerable<Result> results) =>
        new(results.Where(r => r.IsFailure).Select(r => r.Error).ToArray());

    public static ValidationError FromResults<T>(IEnumerable<Result<T>> results) where T : notnull =>
        new(results.Where(r => r.IsFailure).Select(r => r.Error).ToArray());
}
