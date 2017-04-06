

namespace AbiFramework.Entities
{
    public abstract class AEntity<TPrimaryKey> : IEntity<TPrimaryKey>
    {
        public virtual TPrimaryKey Id { get; set; }
    }
}
