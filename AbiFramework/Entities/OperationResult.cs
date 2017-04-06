using System.Collections.Generic;

namespace AbiFramework.Entities
{
    public class OperationResult<T>
    {
        public bool Success { get; set; }
        public T ResultObject { get; set; }
        public List<string> Messages { get; set; } = new List<string>();
    }
}
