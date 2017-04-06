using System;

namespace AbiFramework.Entities
{
    public abstract class AEntityEquatable<TEntity, TPrimaryKey> :
        AEntity<TPrimaryKey>, IEquatable<TEntity> where TEntity : class, IEquatable<TEntity>
    {
        public abstract override int GetHashCode();

        public bool Equals(TEntity other)
        {
            if (other == null) return false;
            return GetHashCode() == other.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as TEntity);
        }

        public static bool operator ==(
            AEntityEquatable<TEntity, TPrimaryKey> g1,
            AEntityEquatable<TEntity, TPrimaryKey> g2)
        {
            if (g1 == null) return false;
            return !(g2 == null) && g1.Equals(g2);
        }

        public static bool operator !=(
            AEntityEquatable<TEntity, TPrimaryKey> g1,
            AEntityEquatable<TEntity, TPrimaryKey> g2)
        {
            return !(g1 == g2);
        }
    }
}
