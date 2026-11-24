using AbiFramework.Entities;

namespace AbiFramework.Extensions;

public static class ResultExtensions
{
    public static TOut Match<TOut>(
        this Result result,
        Func<TOut> onSuccess,
        Func<Result, TOut> onFailure)
    {
        return result switch
        {
            Unit => onSuccess(),
            DomainError => onFailure(result),
            null => onFailure(result)
        };
    }

    public static TOut Match<TIn, TOut>(
        this Result<TIn> result,
        Func<TIn, TOut> onSuccess,
        Func<Result<TIn>, TOut> onFailure)
        where TIn : notnull
    {
        return result switch
        {
            TIn value => onSuccess(value),
            DomainError => onFailure(result),
            null => onFailure(result)
        };
    }
}

