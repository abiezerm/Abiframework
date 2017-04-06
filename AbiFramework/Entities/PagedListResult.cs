using System.Collections.Generic;
using System.Linq;

namespace AbiFramework.Entities
{
    public class PagedListResult<TEntity>
    {
        public long TotalRecords { get; set; }
        public IEnumerable<TEntity> Entities { get; set; } = Enumerable.Empty<TEntity>();
    }
}
