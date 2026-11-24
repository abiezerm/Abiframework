using AbiFramework.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;

namespace AbiFramework.Web;

/// <summary>
/// Converts operation results to typed ASP.NET Core HTTP results.
/// </summary>
public static class ResultHttpExtensions
{
    public static Results<Ok<T>, DomainProblemResult> ToHttpResult<T>(this Result<T> result) where T : notnull => result switch
    {
        T value => TypedResults.Ok(value),
        DomainError error => new DomainProblemResult(error),
        null => new DomainProblemResult(DomainError.NullValue)
    };

    public static Results<TOk, DomainProblemResult> ToHttpResult<T, TOk>(
        this Result<T> result,
        Func<T, TOk> onSuccess)
        where T : notnull
        where TOk : IResult => result switch
    {
        T value => onSuccess(value),
        DomainError error => new DomainProblemResult(error),
        null => new DomainProblemResult(DomainError.NullValue)
    };

    public static Results<Ok, DomainProblemResult> ToHttpResult(this Result result) => result switch
    {
        Unit => TypedResults.Ok(),
        DomainError error => new DomainProblemResult(error),
        null => new DomainProblemResult(DomainError.NullValue)
    };

    public static async Task<Results<Ok<T>, DomainProblemResult>> ToHttpResult<T>(this Task<Result<T>> task)
        where T : notnull => (await task).ToHttpResult();

    public static async Task<Results<TOk, DomainProblemResult>> ToHttpResult<T, TOk>(
        this Task<Result<T>> task,
        Func<T, TOk> onSuccess)
        where T : notnull
        where TOk : IResult => (await task).ToHttpResult(onSuccess);

    public static async Task<Results<Ok, DomainProblemResult>> ToHttpResult(this Task<Result> task) =>
        (await task).ToHttpResult();
}
