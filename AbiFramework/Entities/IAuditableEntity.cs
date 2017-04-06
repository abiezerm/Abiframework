using System;

namespace AbiFramework.Entities
{
    public interface IAuditableEntity<TPrimaryKey> : IEntity<TPrimaryKey> 
    {
        int CreatedBy { get; set; }
        DateTime CreatedDate { get; set; }
        int? UpdatedBy { get; set; }
        DateTime? UpdatedDate { get; set; }
        int? DeletedBy { get; set; }
        DateTime? DeletedDate { get; set; }
    }
}
