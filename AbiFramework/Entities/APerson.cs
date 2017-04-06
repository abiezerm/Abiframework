
namespace AbiFramework.Entities
{
    public abstract class APerson<TPrimaryKey> : AEntity<TPrimaryKey>
    {
        public virtual string FirstName { get; set; }
        public virtual string LastName { get; set; }
    }
}
