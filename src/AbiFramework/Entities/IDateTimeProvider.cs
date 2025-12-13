namespace AbiFramework.Entities;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
