using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using AbiFramework.Entities;

namespace AbiFramework.Repositories
{
    public interface IReadOnlyRepositoryAsync<TEntity, TPrimaryKey> where TEntity : IEntity<TPrimaryKey>
    {
        Task<IQueryable<TEntity>> All();
        Task<TEntity> Find(TPrimaryKey primaryKey);
        Task<TEntity> FindBy(Expression<Func<TEntity, bool>> expression);
        Task<IQueryable<TEntity>> FilterBy(Expression<Func<TEntity, bool>> expression);
        Task<PagedListResult<TEntity>> PagedSearch(Expression<Func<TEntity, bool>> expression, int take, int skip);
        Task<PagedListResult<TEntity>> PagedSearch<TKey>(
            Expression<Func<TEntity, bool>> expression,
            int take,
            int skip,
            Expression<Func<TEntity, TKey>> sortCondition,
            bool sortDesc = false);
    }
}