using System;
using System.Diagnostics.CodeAnalysis;

namespace AbiFramework.Entities;

[SuppressMessage("SonarLint", "S4035", Justification = "Abstract base class is designed to be inherited")]
public abstract class AEntityEquatable<TEntity, TPrimaryKey> :
    AEntity<TPrimaryKey>, IEquatable<TEntity> where TEntity : class, IEquatable<TEntity>
{
    public abstract override int GetHashCode();

    public bool Equals(TEntity? other)
    {
        if (other == null)
        {
            return false;
        }
        return GetHashCode() == other.GetHashCode();
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as TEntity);
    }

    public static bool operator ==(
        AEntityEquatable<TEntity, TPrimaryKey>? g1,
        AEntityEquatable<TEntity, TPrimaryKey>? g2)
    {
        if (g1 is null)
        {
            return g2 is null;
        }
        return g2 is not null && g1.Equals(g2);
    }

    public static bool operator !=(
        AEntityEquatable<TEntity, TPrimaryKey>? g1,
        AEntityEquatable<TEntity, TPrimaryKey>? g2) => !(g1 == g2);
}
