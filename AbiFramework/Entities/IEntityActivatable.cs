namespace AbiFramework.Entities
{
    public interface IEntityActivatable <TPrimaryKey> : IEntity<TPrimaryKey>
    {
        bool IsActive { get; set; }
    }
}