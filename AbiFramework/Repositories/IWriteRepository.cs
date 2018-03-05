using System.Collections.Generic;
using AbiFramework.Entities;

namespace AbiFramework.Repositories
{
    public interface IRepository<TEntity, in TPrimaryKey> : IReadOnlyRepository<TEntity, TPrimaryKey>
        where TEntity : IEntity<TPrimaryKey>
    {
        void Add(TEntity entity);
        void Add(IEnumerable<TEntity> entities);
        
        void Remove(TEntity entity);
        void Remove(IEnumerable<TEntity> entities);

        void Update(TEntity entity);
    }
}
