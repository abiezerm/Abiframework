using System;
using System.Linq;
using System.Linq.Expressions;
using AbiFramework.Entities;

namespace AbiFramework.Repositories
{
    public interface IReadOnlyRepository<TEntity, TPrimaryKey>
       where TEntity : IEntity<TPrimaryKey>
    {
        IQueryable<TEntity> All();
        TEntity Find(TPrimaryKey primaryKey);
        IQueryable<TEntity> FilterBy(Expression<Func<TEntity, bool>> expression);

        PagedListResult<TEntity> PagedSearch(
            Expression<Func<TEntity, bool>> expression, 
            int take, 
            int skip);

        PagedListResult<TEntity> PagedSearch<TKey>(
            Expression<Func<TEntity, bool>> expression,
            int take, 
            int skip, 
            Expression<Func<TEntity, TKey>> sortCondition, 
            bool sortDesc = false);
    }
}
