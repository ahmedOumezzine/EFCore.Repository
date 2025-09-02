using AhmedOumezzine.EFCore.Repository.Entities;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;
using System.Linq.Expressions;

namespace AhmedOumezzine.EFCore.Repository.Interface
{
    /// <summary>
    /// Generic repository interface for CRUD operations, transactions, and advanced querying.
    /// Extends <see cref="IQueryRepository"/> for comprehensive data access capabilities.
    /// </summary>
    public interface IRepository : IQueryRepository
    {
        #region Basic Inserts

        Task<object[]> InsertAsync<TEntity>(
            TEntity entity,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity;

        Task InsertRangeAsync<TEntity>(
            IEnumerable<TEntity> entities,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity;

        Task<TEntity> InsertAndReturnAsync<TEntity>(
            TEntity entity,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity;

        #endregion

        #region Advanced Inserts

        Task BulkInsertAsync<TEntity>(
            IEnumerable<TEntity> entities,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity;

        Task InsertManyAsync<TEntity>(
            IEnumerable<TEntity> entities,
            int batchSize = 500,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity;

        Task<object[]> InsertWithAuditAsync<TEntity>(
            TEntity entity,
            string userName,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity;

        Task<bool> InsertIfNotExistsAsync<TEntity>(
            Expression<Func<TEntity, bool>> predicate,
            TEntity entity,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity;

        Task<bool> TryInsertAsync<TEntity>(
            TEntity entity,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity;

        Task UpsertAsync<TEntity>(
            Expression<Func<TEntity, bool>> predicate,
            TEntity entity,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity;

        Task InsertGraphAsync<TEntity>(
            TEntity entity,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity;

        Task InsertWithTransactionAsync<TEntity>(
            IEnumerable<TEntity> entities,
            CancellationToken cancellationToken = default)
            where TEntity : BaseEntity;

        #endregion



        #region Update

        /// <summary>
        /// Marks an entity as modified.
        /// </summary>
        void Update<TEntity>(TEntity entity) where TEntity : BaseEntity;

        /// <summary>
        /// Marks a collection of entities as modified.
        /// </summary>
        void Update<TEntity>(IEnumerable<TEntity> entities) where TEntity : BaseEntity;

        /// <summary>
        /// Marks an entity as modified and returns the number of affected rows after saving.
        /// </summary>
        Task<int> UpdateAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
            where TEntity : BaseEntity;

        /// <summary>
        /// Marks a collection of entities as modified and returns the number of affected rows after saving.
        /// </summary>
        Task<int> UpdateAsync<TEntity>(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
            where TEntity : BaseEntity;

        #endregion Update

        #region Delete

        /// <summary>
        /// Marks an entity for deletion.
        /// </summary>
        void Remove<TEntity>(TEntity entity) where TEntity : BaseEntity;

        /// <summary>
        /// Marks a collection of entities for deletion.
        /// </summary>
        void Remove<TEntity>(IEnumerable<TEntity> entities) where TEntity : BaseEntity;

        /// <summary>
        /// Marks an entity for deletion and returns the number of affected rows after saving.
        /// </summary>
        Task<int> DeleteAsync<TEntity>(TEntity entity, CancellationToken cancellationToken = default)
            where TEntity : BaseEntity;

        /// <summary>
        /// Marks a collection of entities for deletion and returns the number of affected rows after saving.
        /// </summary>
        Task<int> DeleteAsync<TEntity>(IEnumerable<TEntity> entities, CancellationToken cancellationToken = default)
            where TEntity : BaseEntity;

        #endregion Delete

        #region Save

        /// <summary>
        /// Saves all changes made in this context to the database.
        /// </summary>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The number of affected rows.</returns>
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

        #endregion Save

        #region Raw SQL Commands (Modern & Safe)

        /// <summary>
        /// Executes a raw SQL command (INSERT, UPDATE, DELETE) and returns the number of affected rows.
        /// </summary>
        /// <param name="sql">The SQL command to execute.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The number of affected rows.</returns>
        Task<int> ExecuteSqlCommandAsync(
            string sql,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes a parameterized raw SQL command.
        /// </summary>
        /// <param name="sql">The SQL command with parameters (e.g., 'UPDATE Users SET Name = {0}').</param>
        /// <param name="parameters">The parameters to pass into the command.</param>
        /// <returns>The number of affected rows.</returns>
        Task<int> ExecuteSqlCommandAsync(
            string sql,
            params object[] parameters);

        /// <summary>
        /// Executes a parameterized raw SQL command with cancellation support.
        /// </summary>
        /// <param name="sql">The SQL command with parameters.</param>
        /// <param name="parameters">The parameters to pass into the command.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The number of affected rows.</returns>
        Task<int> ExecuteSqlCommandAsync(
            string sql,
            IEnumerable<object> parameters,
            CancellationToken cancellationToken = default);

        #endregion Raw SQL Commands (Modern & Safe)

        #region Scalar & Single SQL Results

        /// <summary>
        /// Executes a SQL query and returns a single scalar value (e.g., COUNT, MAX).
        /// </summary>
        /// <typeparam name="T">The type of the scalar result.</typeparam>
        /// <param name="sql">The SQL query (e.g., 'SELECT COUNT(*) FROM Users').</param>
        /// <param name="parameters">Optional parameters.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The scalar value, or default(T) if no result.</returns>
        Task<T> ExecuteScalarAsync<T>(
            string sql,
            IEnumerable<object> parameters = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes a raw SQL query and returns the first entity or null.
        /// </summary>
        /// <typeparam name="T">The entity type.</typeparam>
        /// <param name="sql">The SQL query.</param>
        /// <param name="parameters">Optional parameters.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>The first entity or null.</returns>
        Task<T> GetSingleFromSqlAsync<T>(
            string sql,
            IEnumerable<object> parameters = null,
            CancellationToken cancellationToken = default)
            where T : class;

        /// <summary>
        /// Checks if any row matches the given SQL query.
        /// </summary>
        /// <param name="sql">The SQL query that returns a count.</param>
        /// <param name="parameters">Optional parameters.</param>
        /// <param name="cancellationToken">Optional cancellation token.</param>
        /// <returns>True if at least one row matches; otherwise, false.</returns>
        Task<bool> ExistsBySqlAsync(
            string sql,
            IEnumerable<object> parameters = null,
            CancellationToken cancellationToken = default);

        #endregion Scalar & Single SQL Results

        #region Bulk Operations (EF Core 7+)

        /// <summary>
        /// Updates entities matching a condition without loading them into memory (EF Core 7+).
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="predicate">The condition to match (e.g., x => x.Status == "Pending").</param>
        /// <param name="updateAction">The update operation (e.g., x => x.SetProperty(u => u.Status, "Processed")).</param>
        /// <returns>The number of affected rows.</returns>
        Task<int> UpdateFromQueryAsync<TEntity>(
            Expression<Func<TEntity, bool>> predicate,
            Expression<Func<SetPropertyCalls<TEntity>, SetPropertyCalls<TEntity>>> updateAction)
            where TEntity : BaseEntity;

        /// <summary>
        /// Deletes entities matching a condition without loading them into memory (EF Core 7+).
        /// </summary>
        /// <typeparam name="TEntity">The entity type.</typeparam>
        /// <param name="predicate">The condition to match.</param>
        /// <returns>The number of affected rows.</returns>
        Task<int> DeleteFromQueryAsync<TEntity>(Expression<Func<TEntity, bool>> predicate)
            where TEntity : BaseEntity;

        #endregion Bulk Operations (EF Core 7+)

        #region Change Tracker

        /// <summary>
        /// Clears the change tracker. Use with caution: unsaved changes will be lost.
        /// </summary>
        void ClearChangeTracker();

        #endregion Change Tracker
    }
}