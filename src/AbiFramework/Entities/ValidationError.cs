namespace AbiFramework.Entities;

#pragma warning disable CS0618 // Type or member is obsolete
public sealed record ValidationError(Error[] Errors) : Error("Validation.General",
    "One or more validation errors occurred",
    ErrorType.Validation)
{
    public static ValidationError FromResults(IEnumerable<Result> results) =>
        new(results.Where(r => r.IsFailure).Select(r => r.Error).ToArray());
}
#pragma warning restore CS0618 // Type or member is obsolete
