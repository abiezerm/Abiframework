using System.Runtime.CompilerServices;

namespace AbiFramework.Entities;

[Union]
public struct Result : IUnion, IEquatable<Result>
{
    public Result(Unit value) => Value = value;

    public Result(DomainError value) => Value = RequireFailure(value);

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

    internal static DomainError RequireFailure(DomainError? error)
    {
        ArgumentNullException.ThrowIfNull(error);

        return error == DomainError.None
            ? throw new ArgumentException("Invalid error", nameof(error))
            : error;
    }

    public static Result Success() => Unit.Value;

    public static Result<TValue> Success<TValue>(TValue value) where TValue : notnull => value;

    public static Result Failure(DomainError error) => error;

    public static Result<TValue> Failure<TValue>(DomainError error) where TValue : notnull => error;
}

/// <summary>
/// A union of a success value (<typeparamref name="TValue"/>) or a failure (<see cref="DomainError"/>).
/// </summary>
/// <remarks>
/// The case is tracked by an explicit tag, never by a runtime type test of the payload, so
/// <c>Result&lt;object&gt;</c> and <c>Result&lt;DomainError&gt;</c> keep successes and failures distinct in
/// <see cref="IsSuccess"/>, <see cref="Error"/>, <see cref="Value"/>, equality, <c>Match</c> and the HTTP adapters.
/// <para>
/// The exhaustive <c>switch</c> contract (one <typeparamref name="TValue"/> arm and one <see cref="DomainError"/> arm,
/// with missing arms reported by the compiler) applies only when <typeparamref name="TValue"/> does not overlap
/// <see cref="DomainError"/>: it is not <see cref="DomainError"/>, a type derived from it, or a supertype of it such as
/// <see cref="object"/>. For overlapping types, branch on <see cref="IsSuccess"/> / <see cref="IsFailure"/> or use
/// <c>Match</c>. A <c>switch</c> on an overlapping type is not reliable: the case-aware <see cref="IUnionMembers"/>
/// accessors route a <see cref="DomainError"/>-first switch by case, but an arm for a supertype such as
/// <see cref="object"/> also matches failures, and the compiler cannot report the missing case.
/// </para>
/// The library itself never relies on pattern matching over the payload.
/// </remarks>
[Union]
public struct Result<TValue> : Result<TValue>.IUnionMembers, IEquatable<Result<TValue>> where TValue : notnull
{
    private readonly object? _payload;

    private Result(TValue? value)
    {
        if (value is null)
        {
            _payload = DomainError.NullValue;
            return;
        }

        _payload = value;
        IsSuccess = true;
    }

    private Result(DomainError error)
    {
        _payload = Result.RequireFailure(error);
    }

    public interface IUnionMembers
    {
        static Result<TValue> Create(TValue value) => new(value);

        static Result<TValue> Create(DomainError value) => new(value);

        object? Value { get; }

        bool HasValue { get; }

        bool TryGetValue(out TValue value);

        bool TryGetValue(out DomainError value);
    }

    object? IUnionMembers.Value => _payload;

    bool IUnionMembers.HasValue => _payload is not null;

    bool IUnionMembers.TryGetValue(out TValue value) => TryGetValue(out value);

    bool IUnionMembers.TryGetValue(out DomainError value) => TryGetValue(out value);

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public DomainError Error => IsSuccess
        ? DomainError.None
        : _payload as DomainError ?? DomainError.NullValue;

    public TValue Value => IsSuccess
        ? (TValue)_payload!
        : throw new InvalidOperationException("The value of a failure result can't be accessed.");

    /// <summary>Case-aware pattern member: succeeds only for a success result.</summary>
    public bool TryGetValue(out TValue value)
    {
        value = IsSuccess ? (TValue)_payload! : default!;
        return IsSuccess;
    }

    /// <summary>Case-aware pattern member: succeeds only for a failure result.</summary>
    public bool TryGetValue(out DomainError error)
    {
        bool isFailure = !IsSuccess && _payload is DomainError;
        error = isFailure ? (DomainError)_payload! : default!;
        return isFailure;
    }

    public bool Equals(Result<TValue> other) =>
        IsSuccess == other.IsSuccess && Equals(_payload, other._payload);

    public override bool Equals(object? obj) => obj is Result<TValue> other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(IsSuccess, _payload);

    public static bool operator ==(Result<TValue> left, Result<TValue> right) => left.Equals(right);

    public static bool operator !=(Result<TValue> left, Result<TValue> right) => !left.Equals(right);

    public static Result<TValue> Success(TValue value) => new(value);

    public static Result<TValue> Failure(DomainError error) => new(error);

    public static Result<TValue> ValidationFailure(DomainError error) => new(error);
}
