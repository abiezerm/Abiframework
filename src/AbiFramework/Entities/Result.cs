using System.Runtime.CompilerServices;

namespace AbiFramework.Entities;

[Union]
public struct Result : IUnion, IEquatable<Result>
{
    public Result(Unit value) => Value = value;

    public Result(DomainError value) => Value = value;

    public object? Value { get; }

    public bool Equals(Result other) => Equals(Value, other.Value);

    public override bool Equals(object? obj) => obj is Result other && Equals(other);

    public override int GetHashCode() => Value?.GetHashCode() ?? 0;

    public static bool operator ==(Result left, Result right) => left.Equals(right);

    public static bool operator !=(Result left, Result right) => !left.Equals(right);

    public bool IsSuccess => Value is Unit;

    public bool IsFailure => !IsSuccess;

    public DomainError Error => Value switch
    {
        DomainError error => error,
        null => DomainError.NullValue,
        _ => DomainError.None
    };

    public static Result Success() => Unit.Value;

    public static Result<TValue> Success<TValue>(TValue value) where TValue : notnull => value;

    public static Result Failure(DomainError error) => error;

    public static Result<TValue> Failure<TValue>(DomainError error) where TValue : notnull => error;
}

[Union]
public struct Result<TValue> : Result<TValue>.IUnionMembers, IEquatable<Result<TValue>> where TValue : notnull
{
    private readonly object? _value;

    private Result(object? value) => _value = value;

    public interface IUnionMembers
    {
        static Result<TValue> Create(TValue value) => new(value);

        static Result<TValue> Create(DomainError value) => new(value);

        object? Value { get; }
    }

    object? IUnionMembers.Value => _value;

    public bool Equals(Result<TValue> other) => Equals(_value, other._value);

    public override bool Equals(object? obj) => obj is Result<TValue> other && Equals(other);

    public override int GetHashCode() => _value?.GetHashCode() ?? 0;

    public static bool operator ==(Result<TValue> left, Result<TValue> right) => left.Equals(right);

    public static bool operator !=(Result<TValue> left, Result<TValue> right) => !left.Equals(right);

    public bool IsSuccess => _value is TValue;

    public bool IsFailure => !IsSuccess;

    public DomainError Error => _value switch
    {
        DomainError error => error,
        null => DomainError.NullValue,
        _ => DomainError.None
    };

    public TValue Value => _value is TValue value
        ? value
        : throw new InvalidOperationException("The value of a failure result can't be accessed.");

    public static Result<TValue> Success(TValue value) => value;

    public static Result<TValue> Failure(DomainError error) => error;

    public static Result<TValue> ValidationFailure(DomainError error) => error;
}
