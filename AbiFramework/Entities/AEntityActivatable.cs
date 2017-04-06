namespace AbiFramework.Entities
{
    public class AEntityActivatable<TPrimaryKey> : AEntity<TPrimaryKey>, IEntityActivatable<TPrimaryKey>
    {
        public bool IsActive { get; set; }
    }
}