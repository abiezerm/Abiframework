
using System;
using System.Threading.Tasks;

namespace AbiFramework.UnitsOfWork
{
    /// <summary>
    /// Represents a transactional job.
    /// </summary>
    public interface IUnitOfWork : IDisposable
    {
        /// <summary>
        /// Opens database connection and begins transaction.
        /// </summary>
        void BeginTransaction();

        /// <summary>
        /// Commits transaction and closes database connection.
        /// </summary>
        void Commit();

        /// <summary>
        /// Save changes to the database.
        /// </summary>
        Task CommitAsync();

        /// <summary>
        /// Rollbacks transaction and closes database connection.
        /// </summary>
        void Rollback();
    }
}
