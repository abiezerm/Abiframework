using System.Diagnostics.CodeAnalysis;

namespace AbiFramework.Entities;

public class Result
{
    public Result(bool isSuccess, DomainError error)
    {
        if (isSuccess && error != DomainError.None ||
            !isSuccess && error == DomainError.None)
        {
            throw new ArgumentException("Invalid error", nameof(error));
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public DomainError Error { get; }

    public static Result Success() => new(true, DomainError.None);

    public static Result<TValue> Success<TValue>(TValue value) =>
        new(value, true, DomainError.None);

    public static Result Failure(DomainError error) => new(false, error);

    public static Result<TValue> Failure<TValue>(DomainError error) =>
        new(default, false, error);
}

public class Result<TValue> : Result
{
    public Result(TValue? value, bool isSuccess, DomainError error)
        : base(isSuccess, error)
    {
        Value = value;
    }

    [NotNull]
    public TValue Value => IsSuccess
        ? field!
        : throw new InvalidOperationException("The value of a failure result can't be accessed.");

    public static implicit operator Result<TValue>(TValue? value) =>
        value is not null ? Success(value) : Failure<TValue>(DomainError.NullValue);

    public static Result<TValue> ValidationFailure(DomainError error) =>
        new(default, false, error);
}
