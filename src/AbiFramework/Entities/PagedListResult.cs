using System.Collections.Generic;
using System.Linq;

namespace AbiFramework.Entities;

public class PagedListResult<TEntity>
{
    public long TotalRecords { get; set; }
    public IEnumerable<TEntity> Items { get; set; } =[];   
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }  
    public int TotalPages { get; set; }
    public bool HasNext { get; set; }
    public bool HasPrevious { get; set; }
}
