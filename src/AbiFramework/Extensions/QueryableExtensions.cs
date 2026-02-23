using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;

namespace AbiFramework.Entities;

public static class QueryableExtensions
{
    /// <summary>
    /// Applies sorting to an EF Core-backed query. The sort expression is translated to SQL
    /// and executed in the database.
    /// </summary>
    /// <remarks>
    /// Use this overload when the source is an <see cref="IQueryable{T}"/> from EF Core
    /// (e.g., a <c>DbSet</c> or a filtered query). The sorting is performed server-side.
    /// </remarks>
    /// <typeparam name="T">The type of element in the query.</typeparam>
    /// <param name="query">The queryable to sort.</param>
    /// <param name="sortBy">The field name to sort by (case-insensitive). If null or not found, no sort is applied.</param>
    /// <param name="sortOrder">The sort direction: "ASC" (default) or "DESC" (case-insensitive).</param>
    /// <param name="sortFields">A dictionary mapping uppercase field names to sort expressions.</param>
    /// <returns>The sorted queryable, or the original if no valid sort field was specified.</returns>
    public static IQueryable<T> ApplySorting<T>(
        this IQueryable<T> query,
        string? sortBy,
        string? sortOrder,
        Dictionary<string, Expression<Func<T, object>>> sortFields)
    {
        return GetSortResult(sortBy, sortOrder, sortFields).Apply(query);
    }

    /// <summary>
    /// Applies sorting to an in-memory sequence. The sort expression is compiled and executed in process.
    /// </summary>
    /// <remarks>
    /// Use this overload when the source is <b>not</b> backed by EF Core (e.g., an already-materialized
    /// list or an in-memory collection). For EF Core queries, prefer the <see cref="IQueryable{T}"/> overload
    /// to leverage database-side sorting.
    /// </remarks>
    /// <typeparam name="T">The type of element in the sequence.</typeparam>
    /// <param name="source">The enumerable to sort.</param>
    /// <param name="sortBy">The field name to sort by (case-insensitive). If null or not found, no sort is applied.</param>
    /// <param name="sortOrder">The sort direction: "ASC" (default) or "DESC" (case-insensitive).</param>
    /// <param name="sortFields">A dictionary mapping uppercase field names to sort expressions.</param>
    /// <returns>The sorted enumerable, or the original if no valid sort field was specified.</returns>
    public static IEnumerable<T> ApplySorting<T>(
        this IEnumerable<T> source,
        string? sortBy,
        string? sortOrder,
        Dictionary<string, Expression<Func<T, object>>> sortFields)
    {
        return GetSortResult(sortBy, sortOrder, sortFields).Apply(source);
    }

    /// <summary>
    /// Applies pagination asynchronously to an EF Core-backed query. The count and Skip/Take
    /// operations are translated to SQL and executed in the database.
    /// </summary>
    /// <remarks>
    /// Use this overload when the source is an <see cref="IQueryable{T}"/> from EF Core.
    /// Both the total count and the page slice are retrieved server-side for optimal performance.
    /// </remarks>
    /// <typeparam name="T">The type of element in the query.</typeparam>
    /// <param name="query">The queryable to paginate.</param>
    /// <param name="page">The 1-based page number. Values less than 1 are normalized to 1.</param>
    /// <param name="pageSize">The number of items per page (1–100). Values outside this range are clamped.</param>
    /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
    /// <returns>A <see cref="PagedResponse{T}"/> containing the page items and pagination metadata.</returns>
    public static async Task<PagedListResult<T>> ApplyPaginationAsync<T>(
        this IQueryable<T> query,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        (page, pageSize) = NormalizePageAndSize(page, pageSize);

        int totalCount = await query.CountAsync(cancellationToken).ConfigureAwait(false);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        return CreatePagedResponse(items, totalCount, page, pageSize);
    }

    /// <summary>
    /// Applies pagination synchronously to an in-memory sequence. Counting and slicing are performed in process.
    /// </summary>
    /// <remarks>
    /// Use this overload when the source is <b>not</b> backed by EF Core (e.g., an already-materialized
    /// list or an in-memory collection). For EF Core queries, prefer <see cref="ApplyPaginationAsync{T}"/>
    /// to leverage database-side counting and paging.
    /// </remarks>
    /// <typeparam name="T">The type of element in the sequence.</typeparam>
    /// <param name="source">The enumerable to paginate.</param>
    /// <param name="page">The 1-based page number. Values less than 1 are normalized to 1.</param>
    /// <param name="pageSize">The number of items per page (1–100). Values outside this range are clamped.</param>
    /// <returns>A <see cref="PagedListResult{T}"/> containing the page items and pagination metadata.</returns>
    public static PagedListResult<T> ApplyPagination<T>(
        this IEnumerable<T> source,
        int page,
        int pageSize)
    {
        (page, pageSize) = NormalizePageAndSize(page, pageSize);

        var list = source as IReadOnlyList<T> ?? source.ToList();
        int totalCount = list.Count;

        var items = list
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return CreatePagedResponse(items, totalCount, page, pageSize);
    }

    private static SortResult<T> GetSortResult<T>(
        string? sortBy,
        string? sortOrder,
        Dictionary<string, Expression<Func<T, object>>> sortFields)
    {
        if (string.IsNullOrWhiteSpace(sortBy) || !sortFields.TryGetValue(sortBy.ToUpperInvariant(), out var expr))
        {
            return SortResult<T>.None;
        }

        bool descending = sortOrder?.ToUpperInvariant() == "DESC";
        return SortResult<T>.Create(expr, descending);
    }

    private static (int page, int pageSize) NormalizePageAndSize(int page, int pageSize)
    {
        page = Math.Max(1, page);
        pageSize = Math.Max(1, Math.Min(pageSize, 100)); // Limit max page size to 100
        return (page, pageSize);
    }

    private static PagedListResult<T> CreatePagedResponse<T>(IReadOnlyList<T> items, int totalCount, int page, int pageSize)
    {
        int totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
        return new PagedListResult<T>
        {
            Items = items,
            TotalRecords = totalCount,
            CurrentPage = page,
            PageSize = pageSize,
            TotalPages = totalPages,
            HasNext = page < totalPages,
            HasPrevious = page > 1
        };
    }
}
