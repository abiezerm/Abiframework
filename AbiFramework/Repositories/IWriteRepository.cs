using System.Collections.Generic;
using AbiFramework.Entities;

namespace AbiFramework.Repositories
{
    public interface IRepository<TEntity,TPrimaryKey> : IReadOnlyRepository<TEntity, TPrimaryKey>
        where TEntity : IEntity<TPrimaryKey>
    {
        void Add(TEntity entity);
        void AddRange(IEnumerable<TEntity> entities);
        
        void Remove(TEntity entity);
        void RemoveRange(IEnumerable<TEntity> entities);
    }
}
