using System.Linq.Expressions;

namespace AbiFramework.Entities;

/// <summary>
/// Represents the result of resolving a sort field, containing either a valid sort expression
/// and direction or no sort information.
/// </summary>
/// <typeparam name="T">The type of element being sorted.</typeparam>
internal readonly struct SortResult<T>
{
    /// <summary>
    /// Gets a value indicating whether a valid sort expression was resolved.
    /// </summary>
    public bool HasValue { get; }

    /// <summary>
    /// Gets the sort expression, or null if <see cref="HasValue"/> is false.
    /// </summary>
    public Expression<Func<T, object>>? Expression { get; }

    /// <summary>
    /// Gets a value indicating whether the sort should be descending.
    /// </summary>
    public bool Descending { get; }

    private SortResult(Expression<Func<T, object>>? expression, bool descending, bool hasValue)
    {
        Expression = expression;
        Descending = descending;
        HasValue = hasValue;
    }

    /// <summary>
    /// Creates a <see cref="SortResult{T}"/> representing no sort.
    /// </summary>
    public static SortResult<T> None => new(null, false, false);

    /// <summary>
    /// Creates a <see cref="SortResult{T}"/> with a valid sort expression and direction.
    /// </summary>
    /// <param name="expression">The sort expression.</param>
    /// <param name="descending">True for descending order; false for ascending.</param>
    public static SortResult<T> Create(Expression<Func<T, object>> expression, bool descending)
        => new(expression, descending, true);

    /// <summary>
    /// Applies the sort to an EF Core-backed query. If no sort was resolved, returns the query unchanged.
    /// </summary>
    /// <param name="query">The queryable to sort.</param>
    /// <returns>The sorted queryable, or the original if no sort was resolved.</returns>
    public IQueryable<T> Apply(IQueryable<T> query)
    {
        if (!HasValue || Expression is null)
        {
            return query;
        }

        return Descending ? query.OrderByDescending(Expression) : query.OrderBy(Expression);
    }

    /// <summary>
    /// Applies the sort to an in-memory sequence. If no sort was resolved, returns the source unchanged.
    /// </summary>
    /// <param name="source">The enumerable to sort.</param>
    /// <returns>The sorted enumerable, or the original if no sort was resolved.</returns>
    public IEnumerable<T> Apply(IEnumerable<T> source)
    {
        if (!HasValue || Expression is null)
        {
            return source;
        }

        var selector = Expression.Compile();
        return Descending ? source.OrderByDescending(selector) : source.OrderBy(selector);
    }
}
