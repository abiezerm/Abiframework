using System;

namespace AbiFramework.Entities
{
    public abstract class AEntityAuditable<TPrimaryKey> 
        : AEntity<TPrimaryKey>, IAuditableEntity<TPrimaryKey>
    {
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.Now;
        public int? UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime? DeletedDate { get; set; }
    }
}
