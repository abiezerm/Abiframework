namespace AbiFramework.Entities;

public sealed class Unit
{
    private Unit()
    {
    }

    public static Unit Value { get; } = new();
}
